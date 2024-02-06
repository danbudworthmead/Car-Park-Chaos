using System;
using System.Linq;
using _Game.Car.Player;
using _Game.Car.Player_Cars;
using Scenes.Online;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Game_States_Logic
{
    public class MatchManager : NetworkBehaviour
    {
        [SerializeField] private NetworkObject roundLogicPrefab;
        
        public float RoundDuration = 60;

        public RoundManager CurrentRound { get; private set; }
        public static MatchManager Singleton { get; private set; }
        public RoundManager.RoundStates RoundState => CurrentRound ? CurrentRound.RoundState : RoundManager.RoundStates.Initializing;

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
                        // check if all players are connected
                        // ReSharper disable once ReplaceWithSingleAssignment.True
                        if (RelayManager.Instance == null
                            || RelayManager.Instance.PlayersInLobby.Count >
                            NetworkManager.Singleton.ConnectedClients.Count)
                        {
                            if (NetworkManager.Singleton.ConnectedClients.Count == 1)
                            {
                                // single player fix
                                var client = NetworkManager.Singleton.LocalClient;
                                var chosenCarIdx = client.PlayerObject.GetComponent<PlayerData>().carChoice.Value;
                                client.PlayerObject.GetComponent<Player>().InstantiateCarClientRpc(chosenCarIdx);
                                SetMatchStateClientRpc(MatchState.Playing);
                            }
                            return;
                        }

                        // check all players have players states
                        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
                        {
                            if (client.PlayerObject == null)
                                return;

                            if (client.PlayerObject.GetComponent<PlayerState>() == null)
                                return;
                            
                            if (client.PlayerObject.GetComponent<PlayerData>() == null)
                                return;
                            
                            if (client.PlayerObject.GetComponent<PlayerData>().carChoice.Value == -1)
                                return;
                        }
                        
                        // spawn the players cars
                        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
                        {
                            var chosenCarIdx = client.PlayerObject.GetComponent<PlayerData>().carChoice.Value;
                            client.PlayerObject.GetComponent<Player>().InstantiateCarClientRpc(chosenCarIdx);
                        }

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
    }
}
