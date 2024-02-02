using System;
using System.Linq;
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
                        var allConnected = RelayManager.Instance == null
                                           || RelayManager.Instance.PlayersInLobby.Count == NetworkManager.Singleton.ConnectedClients.Count();

                        if (allConnected)
                        {
                            Init();
                            SetMatchStateClientRpc(MatchState.Playing);
                        }
                        break;
                    case MatchState.Playing:
                        break;
                    case MatchState.GameOver:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Init()
        {
            // spawn the round logic
            var obj = Instantiate(roundLogicPrefab);
            obj.Spawn();
            _currentRound = obj.GetComponent<RoundManager>();
        }

        [ClientRpc]
        private void SetMatchStateClientRpc(MatchState matchState)
        {
            _matchState = matchState;
        }
    }
}
