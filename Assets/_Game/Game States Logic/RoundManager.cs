using System;
using _Game.Car;
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
            
            switch (_roundState)
            {
                case RoundState.Initializing:
                    if (NetworkManager.Singleton.IsHost)
                    {
                        // check all players have a car
                        foreach (var client in NetworkManager.Singleton.ConnectedClients)
                        {
                            if (client.Value.PlayerObject == null)
                            {
                                return;
                            }
                        }
                        
                        // teleport all players to their starting positions
                        foreach (var client in NetworkManager.Singleton.ConnectedClients)
                        {
                            var physics = client.Value.PlayerObject.GetComponent<CarPhysics>();
                            physics.SetPositionClientRpc(new Vector3(client.Key * 5f, 0, 0));
                        }
                        SetStateClientRpc(RoundState.Countdown);
                    }
                    break;
                case RoundState.Countdown:
                    // do a 3 second countdown
                    break;
                case RoundState.Playing:
                    // wait until all spaces have been taken
                    break;
                case RoundState.GameOver:
                    // knockout the players who are not in a parking space
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        [ClientRpc]
        public void SetStateClientRpc(RoundState state)
        {
            _roundState = state;
        }
    }
}
