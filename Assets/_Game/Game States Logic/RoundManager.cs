using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Car;
using _Game.Car_Park;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Game_States_Logic
{
    public class RoundManager : NetworkBehaviour
    {
        public enum RoundState
        {
            Initializing,
            Countdown,
            Playing,
            GameOver,
        }
        
        private RoundState _roundState;
        private List<CarParkSpace> _parkingSpaces = new();

        private void Start()
        {
            _roundState = RoundState.Initializing;
        }

        private void FixedUpdate()
        {
            if (IsSpawned == false)
            {
                return;
            }

            if (NetworkManager.Singleton.IsHost)
            {
                HostLogic();
            }
            else
            {
                ClientLogic();
            }
        }

        private void ClientLogic()
        {
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void HostLogic()
        {
            switch (_roundState)
            {
                case RoundState.Initializing:
                    // check all players have a car
                    if (!AllPlayersReady())
                    {
                        return;
                    }

                    TeleportPlayers();
                    FreeSpaces();
                    SetStateClientRpc(RoundState.Countdown);
                    break;
                case RoundState.Countdown:
                    // do a 3 second countdown
                    SetStateClientRpc(RoundState.Playing);
                    break;
                case RoundState.Playing:
                    // wait until all spaces have been taken
                    if (!AnyFreeSpaces())
                    {
                        SetStateClientRpc(RoundState.GameOver);
                    }
                    break;
                case RoundState.GameOver:
                    // knockout the players who are not in a parking space
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool AnyFreeSpaces()
        {
            return _parkingSpaces.Any(space => space.IsFree);
        }

        private bool AllPlayersReady()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                if (client.Value.PlayerObject == null) 
                    return false;
            }

            return true;
        }

        private void TeleportPlayers()
        {
            // teleport all players to their starting positions
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                var physics = client.Value.PlayerObject.GetComponent<CarPhysics>();
                physics.SetPositionClientRpc(new Vector3(client.Key * 5f, 0, 0));
            }
        }

        private void FreeSpaces()
        {
            // free up n-1 car parking spaces
            var parkingSpaces = FindObjectsOfType<CarParkSpace>().ToList();
            var numberOfSpacesToFree = NetworkManager.Singleton.ConnectedClients.Count - 1;
            numberOfSpacesToFree = Mathf.Clamp(numberOfSpacesToFree, 1, parkingSpaces.Count);
            parkingSpaces.Shuffle();
                    
            for (var i = 0; i < numberOfSpacesToFree; i++)
            {
                var space = parkingSpaces[i];
                space.SetFreeClientRpc();
                _parkingSpaces.Add(space);
            }
        }

        [ClientRpc]
        public void SetStateClientRpc(RoundState state)
        {
            _roundState = state;
            Debug.Log($"Round state is now {_roundState}");
        }
    }
}
