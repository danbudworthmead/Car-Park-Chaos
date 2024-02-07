using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Car;
using _Game.Car_Park;
using _Game.Car.Player;
using _Game.Car.Player_Cars;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

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
        private float _countdownTimer;

        private void Start()
        {
            timer = MatchManager.Singleton.RoundDuration;
            if (NetworkManager.Singleton.IsHost)
            {
                var alivePlayers = AlivePlayers();
                
                // create a string of all alive players names and their client ids and print it
                var playerNames = string.Join(", ", alivePlayers
                    .Select(p => $"{p.GetComponent<PlayerData>().Player}"));
                Debug.Log($"Alive players: {playerNames}");
                
                var alivePlayersCount = alivePlayers.Count();
                InitClientRpc(UnityEngine.Random.Range(0, int.MaxValue), alivePlayersCount - 1);
            }
        }

        [ClientRpc]
        private void InitClientRpc(int seed, int freeSpaces)
        {
            Debug.Log($"Initializing round with seed {seed} and {freeSpaces} free spaces");
            
            var rng = new Random(seed);
            
            // go through all the spaces and set random cars
            var carParkSpaces = FindObjectOfType<CarParkSpaces>().spaces;
            foreach (var space in carParkSpaces)
            {
                space.SetCar(rng.Next(int.MaxValue));
                
                if (rng.Next(2) == 0)
                {
                    space.NpcCar.transform.Rotate(Vector3.forward, 180);
                }
            }

            // free up random spaces
            carParkSpaces.Shuffle(rng);
            
            for (var i = 0; i < freeSpaces; ++i)
            {
                carParkSpaces[i].Free();
                _parkingSpaces.Add(carParkSpaces[i]);
                Debug.Log($"Space {carParkSpaces[i].NetworkObjectId} is now free");
            }
        }

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
                    _countdownTimer = 3f;
                    SetStateClientRpc(RoundStates.Countdown);
                    NotificationManager.Singleton.NewNotification("Countdown starting!");
                    break;
                case RoundStates.Countdown:
                    // do a 3 second countdown
                    if (_countdownTimer > 0.0f)
                    {
                        _countdownTimer -= Time.deltaTime;
                    }
                    else
                    {
                        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
                        {
                            var state = client.PlayerObject.GetComponent<PlayerState>();
                            if (state.IsAlive)
                            {
                                state.UnfreezeClientRpc();
                            }
                        }

                        NotificationManager.Singleton.NewNotification("Begin game!");
                        SetStateClientRpc(RoundStates.Playing);
                    }
                    break;
                case RoundStates.Playing:
                    // wait until all spaces have been taken or timer has ran out
                    if (!AnyFreeSpaces() || timer <= 0.0f)
                    {
                        var playerIds = NetworkManager.Singleton.ConnectedClients
                            .Values
                            .Where(c => c.PlayerObject.GetComponent<PlayerState>().IsAlive)
                            .Select(c => c.ClientId).ToList();
                        
                        // remove all players in parking spaces (if reason of loss is because of no more free spaces)
                        if(!AnyFreeSpaces())
                        {
                            foreach (var space in _parkingSpaces)   
                            {
                                playerIds.Remove(space.CarInSpace.OwnerClientId);
                            }
                        }

                        if (NetworkManager.Singleton.ConnectedClients.Keys.Count() > 1)
                        {
                            // kill all remaining players
                            foreach (var player in playerIds)
                            {
                                NetworkManager.Singleton.ConnectedClients[player]
                                    .PlayerObject.GetComponent<PlayerState>().SetDeadClientRpc();
                            }

                            SetStateClientRpc(RoundStates.GameOver);
                        }
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
            var spawnLocations = GameObject.FindWithTag("SpawnLocations").transform;
            var clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();
            clientIds.Shuffle();

            for (var i = 0; i < clientIds.Count; ++i)
            {
                var client = NetworkManager.Singleton.ConnectedClients[(ulong)i].PlayerObject;
                var physics = client.GetComponent<CarPhysics>();
                var spawnLocation = spawnLocations.GetChild(i);
                physics.SetPositionRotationClientRpc(spawnLocation.position, spawnLocation.rotation);
            }
        }

        public IEnumerable<PlayerState> AlivePlayers()
        {
            return NetworkManager.Singleton.ConnectedClients.Values
                .Where(c => c.PlayerObject.GetComponent<PlayerState>().IsAlive)
                .Select(c => c.PlayerObject.GetComponent<PlayerState>());
        }

        [ClientRpc]
        public void SetStateClientRpc(RoundStates states)
        {
            RoundState = states;
            Debug.Log($"Round state is now {RoundState}");
        }
    }
}
