using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Car;
using _Game.Car_Park;
using _Game.Car.Player_Cars;
using Unity.Netcode;
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

        public RoundStates RoundState { get; private set; } = RoundStates.Initializing;
        public float timer;
        private readonly List<CarParkSpace> _parkingSpaces = new();

        private void FixedUpdate()
        {
            if (IsSpawned == false)
            {
                return;
            }

            ClientLogic();
            if (NetworkManager.Singleton.IsHost)
            {
                HostLogic();
            }
        }   

        private void ClientLogic()
        {
            switch (RoundState)
            {
                case RoundStates.Initializing:
                    timer = MatchManager.Singleton.RoundDuration;
                    break;
                case RoundStates.Playing:
                    timer -= Time.deltaTime;
                    break;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void HostLogic()
        {
            switch (RoundState)
            {
                case RoundStates.Initializing:
                    SetupPlayers();
                    FreeSpaces();
                    SetStateClientRpc(RoundStates.Countdown);
                    break;
                case RoundStates.Countdown:
                    // do a 3 second countdown
                    SetStateClientRpc(RoundStates.Playing);
                    break;
                case RoundStates.Playing:
                    // wait until all spaces have been taken or timer has ran out
                    if (!AnyFreeSpaces() || timer <= 0.0f)
                    {
                        var playerIds = NetworkManager.Singleton.ConnectedClients.Keys.ToList();
                        
                        // remove all players in parking spaces (if reason of loss is because of no more free spaces)
                        if(!AnyFreeSpaces())
                        {
                            foreach (var space in _parkingSpaces)   
                            {
                                playerIds.Remove(space.CarInSpace.OwnerClientId);
                            }
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

        private void SetupPlayers()
        {
            Transform spawnLocations = GameObject.FindWithTag("SpawnLocations").transform;
            var clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();
            clientIds.Shuffle();

            for (int i = 0; i < clientIds.Count; ++i)
            {
                var client = NetworkManager.Singleton.ConnectedClients[(ulong)i].PlayerObject;
                var physics = client.GetComponent<CarPhysics>();
                var playerState = client.GetComponent<PlayerState>();
                var spawnLocation = spawnLocations.GetChild(i);
                playerState.SetAliveClientRpc();
                physics.SetPositionClientRpc(spawnLocation.position);
                physics.SetRotationClientRpc(spawnLocation.rotation);
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
