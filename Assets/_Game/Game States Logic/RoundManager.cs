using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Car;
using _Game.Car_Park;
using _Game.Car.Player_Cars;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _Game.Game_States_Logic
{
    public class RoundManager : NetworkBehaviour
    {
        public enum RoundStates
        {
            Initializing,
            Countdown,
            Playing,
            GameOver,
        }
        
        public RoundStates RoundState { get; private set; }
        private List<CarParkSpace> _parkingSpaces = new();

        private void Start()
        {
            RoundState = RoundStates.Initializing;
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
            switch (RoundState)
            {
                case RoundStates.Initializing:
                    // check all players have a car
                    if (!AllPlayersReady())
                    {
                        return;
                    }

                    TeleportPlayers();
                    FreeSpaces();
                    SetStateClientRpc(RoundStates.Countdown);
                    break;
                case RoundStates.Countdown:
                    // do a 3 second countdown
                    SetStateClientRpc(RoundStates.Playing);
                    break;
                case RoundStates.Playing:
                    // wait until all spaces have been taken
                    if (!AnyFreeSpaces())
                    {
                        var playerIds = NetworkManager.Singleton.ConnectedClients.Keys.ToList();
                        
                        // remove all players in parking spaces
                        foreach (var space in _parkingSpaces)
                        {
                            playerIds.Remove(space.CarInSpace.OwnerClientId);
                        }
                    
                        // kill all remaining players
                        foreach (var player in playerIds)
                        {
                            NetworkManager.Singleton.ConnectedClients[player]
                                .PlayerObject.GetComponent<PlayerState>().SetDeadClientRpc();
                        }
                        
                        SetStateClientRpc(RoundStates.GameOver);
                    }
                    break;
                case RoundStates.GameOver:
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
            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values.Select(c => c.PlayerObject))
            {
                var physics = client.GetComponent<CarPhysics>();
                physics.SetPositionClientRpc(new Vector3(client.OwnerClientId * 5f, 0, 0));
            }
        }

        public IEnumerable<PlayerState> AlivePlayers()
        {
            return FindObjectsOfType<PlayerState>()
                .Where(p => p.IsAlive);
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
        public void SetStateClientRpc(RoundStates states)
        {
            RoundState = states;
            Debug.Log($"Round state is now {RoundState}");
        }
    }
}
