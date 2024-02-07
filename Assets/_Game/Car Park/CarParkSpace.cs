using System;
using System.Linq;
using _Game.Car;
using _Game.Car.Cars;
using _Game.Car.Player_Cars;
using _Game.Game_States_Logic;
using Unity.Netcode;
using UnityEngine;
#pragma warning disable CS0108, CS0114

namespace _Game.Car_Park
{
    public class CarParkSpace : NetworkBehaviour
    {
        [SerializeField] private BoxCollider collider;
        [SerializeField] private Cars cars;
        
        public CarPhysics CarInSpace { get; private set; }
        public bool IsFree => CarInSpace == null;
        public GameObject NpcCar { get; private set; }

        private void Start()
        {
            name = $"Space ({NetworkObjectId})";
        }

        private void Update()
        {
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
            if (IsFree && NetworkManager.Singleton.IsHost)
            {
                var player = other.GetComponent<PlayerState>();
                if (player && player.IsAlive)
                {
                    if (MatchManager.Singleton.RoundState != RoundManager.RoundStates.Playing)
                    {
                        return;
                    }
                    
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
            Debug.Log($"Car Park Space [{NetworkObjectId}] is now occupied by {carOwnerId}");
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

        public void SetCar(int carIdx)
        {
            if (NpcCar)
            {
                Destroy(NpcCar);
            }
            
            NpcCar = Instantiate(cars.GetCar(carIdx), transform).gameObject;
            NpcCar.transform.position = transform.position;
            
            CarInSpace = null;
            collider.isTrigger = false;
            NpcCar.SetActive(true);
        }

        public void Free()
        {
            CarInSpace = null;
            collider.isTrigger = true;
            NpcCar.SetActive(false);
        }

        public void ResetSpace()
        {
            CarInSpace = null;
            collider.isTrigger = false;
            if (NpcCar)
            {
                NpcCar.SetActive(true);
            }
        }
    }
}