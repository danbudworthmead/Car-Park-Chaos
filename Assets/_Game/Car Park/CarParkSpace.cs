using System.Linq;
using _Game.Car;
using _Game.Car.Player_Cars;
using Unity.Netcode;
using UnityEngine;
#pragma warning disable CS0108, CS0114

namespace _Game.Car_Park
{
    public class CarParkSpace : NetworkBehaviour
    {
        [SerializeField] private BoxCollider collider;
        [SerializeField] private GameObject npcCar;
        
        public CarPhysics CarInSpace { get; private set; }
        public bool IsFree => CarInSpace == null;

        private void Update()
        {
            GetComponentInChildren<MeshRenderer>().material.color = CarInSpace ? Color.green : Color.red;
            
            if (NetworkManager.Singleton.IsHost)
            {
                if (CarInSpace)
                {
                    // check all wheels are in the parking space
                    var wheels = CarInSpace.GetComponentsInChildren<WheelCollider>();
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
                var player = other.GetComponent<PlayerState>();
                if (player && player.IsAlive)
                {
                    // check all wheels are in the parking space
                    var wheels = other.GetComponentsInChildren<WheelCollider>();
                    var inSpace = wheels.All(wheel => collider.bounds.Contains(wheel.transform.position));
                    if (inSpace)
                    {
                        // player has parked
                        SetCarInSpaceClientRpc(player.OwnerClientId);
                    }
                }
            }
        }
        
        [ClientRpc]
        public void SetCarInSpaceClientRpc(ulong carOwnerId)
        {
            if (carOwnerId == ulong.MaxValue)
            {
                CarInSpace = null;
            }
            else
            {
                CarInSpace = FindObjectsOfType<CarPhysics>()
                    .First(car => car.OwnerClientId == carOwnerId);
                CarInSpace.GetComponent<PlayerState>().SetParked();
            }
        }

        [ClientRpc]
        public void SetFreeClientRpc()
        {
            CarInSpace = null;
            collider.isTrigger = true;
            Destroy(npcCar);
        }
    }
}