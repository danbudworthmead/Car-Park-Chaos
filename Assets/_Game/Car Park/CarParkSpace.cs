using System.Linq;
using _Game.Car;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Car_Park
{
    public class CarParkSpace : NetworkBehaviour
    {
        [SerializeField] private BoxCollider collider;
        
        private CarPhysics _carInSpace;

        private void Update()
        {
            GetComponentInChildren<MeshRenderer>().material.color = _carInSpace ? Color.green : Color.red;
            
            if (NetworkManager.Singleton.IsHost)
            {
                if (_carInSpace)
                {
                    // check all wheels are in the parking space
                    var wheels = _carInSpace.GetComponentsInChildren<WheelCollider>();
                    var inSpace = wheels.All(wheel => collider.bounds.Contains(wheel.transform.position));
                    if (!inSpace)
                    {
                        // player has left the parking space
                        SetCarInSpaceClientRpc(ulong.MaxValue);
                    }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                var car = other.GetComponent<CarPhysics>();
                if (car)
                {
                    // check all wheels are in the parking space
                    var wheels = other.GetComponentsInChildren<WheelCollider>();
                    var inSpace = wheels.All(wheel => collider.bounds.Contains(wheel.transform.position));
                    if (inSpace)
                    {
                        // player has parked
                        SetCarInSpaceClientRpc(car.OwnerClientId);
                    }
                }
            }
        }
        
        [ClientRpc]
        public void SetCarInSpaceClientRpc(ulong carOwnerId)
        {
            if (carOwnerId == ulong.MaxValue)
            {
                _carInSpace = null;
            }
            else
            {
                var carObj = NetworkManager.Singleton.ConnectedClients
                    .First(client => client.Key == carOwnerId).Value.PlayerObject;
                _carInSpace = carObj.GetComponent<CarPhysics>();
            }
        }
    }
}