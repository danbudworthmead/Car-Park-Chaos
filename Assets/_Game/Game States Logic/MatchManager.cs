using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Car.Player_Cars;
using Scenes.Online;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Game_States_Logic
{
    public class MatchManager : NetworkBehaviour
    {   
        [SerializeField] private NetworkObject roundLogicPrefab;
        private RoundManager _currentRound;
        
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
            _matchState = MatchState.Initializing;
        }

        private void Update()
        {
            if (_currentRound
                && _currentRound.transform.parent == null
                && _currentRound.IsSpawned)
            {
                _currentRound.transform.SetParent(transform);
            }
            
            if (NetworkManager.Singleton.IsHost)
            {
                switch (_matchState)
                {
                    case MatchState.Initializing:
                        // check if all players are connected
                        // ReSharper disable once ReplaceWithSingleAssignment.True
                        if (RelayManager.Instance.PlayersInLobby.Count >
                            NetworkManager.Singleton.ConnectedClients.Count)
                        {
                            return;
                        }

                        // check all players have cars
                        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
                        {
                            if (client.PlayerObject == null)
                                return;

                            if (client.PlayerObject.GetComponent<PlayerState>() == null)
                                return;
                        }

                        SetMatchStateClientRpc(MatchState.Playing);
                        break;
                    case MatchState.Playing:
                        if (_currentRound)
                        {
                            if (_currentRound.RoundState == RoundManager.RoundStates.GameOver)
                            {
                                _currentRound.GetComponent<NetworkObject>().Despawn();
                                Destroy(_currentRound.gameObject);
                                _currentRound = null;
                            }
                        }
                        else
                        {
                            var players = NetworkManager.Singleton.ConnectedClients.Values
                                .Select(c => c.PlayerObject.GetComponent<PlayerState>());
                            if (players.Count(p => p.IsAlive) > 1)
                            {
                                _currentRound = InitRound();
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
