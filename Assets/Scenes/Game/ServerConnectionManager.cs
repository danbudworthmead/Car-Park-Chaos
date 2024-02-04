using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Car.Player_Cars;
using Scenes.Online;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Scenes.Game
{
    public class ServerConnectionManager : MonoBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private GameObject uiPrefab;
        
        private Dictionary<ulong, NetworkObject> _players = new();
        private bool _hasConnected = false;

        private async void Start()
        {
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientStarted += HandleClientStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;


            if (RelayManager.Instance == null)
            {
                // we're in single player mode
                await HandleSinglePlayer();
                return;
            }
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(RelayManager.Instance.RelayServerData);
            
            if (RelayManager.Instance.AmHost)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        private async Task HandleSinglePlayer()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            var allocation = await RelayService.Instance.CreateAllocationAsync(1);
            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
        }

        private async void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"Client connected: {clientId}");

            if (NetworkManager.Singleton.IsHost)
            {
                var playerCar = Instantiate(playerPrefab);
                playerCar.SpawnAsPlayerObject(clientId);
                while (playerCar.IsSpawned == false)
                {
                    await Task.Yield();
                }
                _players.Add(clientId, playerCar);

                if (RelayManager.Instance)
                {
                    var playersInLobby = RelayManager.Instance.PlayersInLobby;
                    foreach (var player in _players)
                    {
                        var playerLobbyData = playersInLobby[(int)player.Key];
                        player.Value.GetComponent<PlayerData>()
                            .SetClientRpc(playerLobbyData.Data["PlayerName"].Value);
                    }
                }
            }

            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                Instantiate(uiPrefab);
                _hasConnected = true;
            }
        }

        private void HandleServerStarted()
        {
        }

        private void HandleClientStarted()
        {
        }

        private void Update()
        {
            if (_hasConnected && !NetworkManager.Singleton.IsConnectedClient)
            {
                // if we disconnect from the server, go back to the online menu
                SceneManager.LoadScene("Online");
            }
        }
    }
}
