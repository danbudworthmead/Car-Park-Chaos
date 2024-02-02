using System.Linq;
using _Game.Car;
using Unity.Netcode;
using UnityEngine;
#pragma warning disable CS0108, CS0114

namespace _Game.Car_Park
{
    public class CarParkSpace : NetworkBehaviour
    {
        [SerializeField] private BoxCollider collider;
        [SerializeField] private GameObject npcCar;
        
        private CarPhysics _carInSpace;
        public bool IsFree => _carInSpace == null;

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
                _carInSpace = FindObjectsOfType<CarPhysics>()
                    .First(car => car.OwnerClientId == carOwnerId);
            }
        }

        [ClientRpc]
        public void SetFreeClientRpc()
        {
            _carInSpace = null;
            collider.isTrigger = true;
            Destroy(npcCar);
        }
    }
}