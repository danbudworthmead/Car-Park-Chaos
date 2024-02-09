using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Car.Cars;
using _Game.Car.Player;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Game_States_Logic
{
    public class MatchManager : NetworkBehaviour
    {
        [SerializeField] private NetworkObject roundLogicPrefab;
        
        private List<Player> _players = new();

        public float RoundDuration = 60;

        public RoundManager CurrentRound { get; private set; }
        public static MatchManager Singleton { get; private set; }
        public RoundManager.RoundStates RoundState => CurrentRound ? CurrentRound.RoundState : RoundManager.RoundStates.Initializing;

        [SerializeField] private Cars cars;

        enum MatchState
        {
            Initializing,
            Playing,
            GameOver,
        }
        
        private MatchState _matchState;
        private int _roundNumber;

        private void Start()
        {
            Singleton = this;
            _matchState = MatchState.Initializing;
        }

        private void Update()
        {
            if (CurrentRound
                && CurrentRound.transform.parent == null
                && CurrentRound.IsSpawned)
            {
                CurrentRound.transform.SetParent(transform);
            }

            if (NetworkManager.Singleton.IsHost)
            {
                switch (_matchState)
                {
                    case MatchState.Initializing:
                        InitPlayers();
                        SpawnCars();
                        SetMatchStateClientRpc(MatchState.Playing);
                        break;
                    case MatchState.Playing:
                        if (CurrentRound)
                        {
                            if (CurrentRound.RoundState == RoundManager.RoundStates.GameOver)
                            {
                                CurrentRound.GetComponent<NetworkObject>().Despawn();
                                Destroy(CurrentRound.gameObject);
                                CurrentRound = null;
                            }
                        }
                        else
                        {
                            if (AtLeastOnePlayerAlive())
                            {
                                CurrentRound = InitRound();
                            } 
                            else
                            {
                                SetMatchStateClientRpc(MatchState.GameOver);
                            }
                        }
                        break;
                    case MatchState.GameOver:
                        // stop the server and disconnect all clients
                        NetworkManager.Singleton.Shutdown();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (CurrentRound == null)
                CurrentRound = GetComponentInChildren<RoundManager>();

        }

        private void SpawnCars()
        {
            foreach (var player in _players)
            {
                var carIdx = UnityEngine.Random.Range(0, int.MaxValue);
                var carPrefab = cars.GetCar(carIdx);
                var carObject = Instantiate(carPrefab);
                var netObj = carObject.GetComponent<NetworkObject>();
                netObj.SpawnAsPlayerObject(player.ClientId);
            }
        }

        private void InitPlayers()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                var player = new Player
                {
                    IsAlive = true,
                    ClientId = client.ClientId
                };
                _players.Add(player);
            }
        }

        private bool AtLeastOnePlayerAlive()
        {
            var onePlayerAlive = false;
            
            var players = NetworkManager.Singleton.ConnectedClients.Values
                .Select(c => c.PlayerObject.GetComponent<PlayerState>());
            var playerStates = players as PlayerState[] ?? players.ToArray();

            if (playerStates.Count() == 1 || playerStates.Count(p => p.IsAlive) > 1)
            {
                onePlayerAlive = true;
            }
            
            return onePlayerAlive;
        }

        private RoundManager InitRound()
        {
            // spawn the round logic
            var obj = Instantiate(roundLogicPrefab);
            obj.Spawn();
            _roundNumber++;
            Debug.Log($"Round {_roundNumber} started");
            return obj.GetComponent<RoundManager>();
        }

        [ClientRpc]
        private void SetMatchStateClientRpc(MatchState matchState)
        {
            _matchState = matchState;
            Debug.Log($"Match state: {_matchState}");
        }

        public IEnumerable<Player> GetAlivePlayers()
        {
            return _players.Where(p => p.IsAlive);
        }

        public bool IsAlive(ulong clientClientId)
        {
            return _players.Any(p => p.ClientId == clientClientId && p.IsAlive);
        }
    }
}
