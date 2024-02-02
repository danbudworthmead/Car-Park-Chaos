using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Camera;
using _Game.Car.Player_Cars;
using Scenes.Online;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Scenes.Game
{
    public class ServerConnectionManager : MonoBehaviour
    {
        public static ServerConnectionManager Instance;

        [SerializeField] private Transform wideCamera;
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private NetworkObject playerPrefab;
        
        private Dictionary<ulong, NetworkObject> _players = new();

        private void Start()
        {
            Instance = this;
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(RelayManager.Instance.RelayServerData);

            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientStarted += HandleClientStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            
            if (RelayManager.Instance.AmHost)
            {
                NetworkManager.Singleton.StartHost();   
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        private async void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"Client connected: {clientId}");

            if (NetworkManager.Singleton.IsHost)
            {
                var playersInLobby = RelayManager.Instance.PlayersInLobby;

                var playerCar = Instantiate(playerPrefab);
                playerCar.SpawnAsPlayerObject(clientId);
                while (playerCar.IsSpawned == false)
                {
                    await Task.Yield();
                }
                _players.Add(clientId, playerCar);

                foreach (var player in _players)
                {
                    var playerLobbyData = playersInLobby[(int)player.Key];
                    player.Value.GetComponent<PlayerData>()
                        .SetClientRpc(playerLobbyData.Data["PlayerName"].Value);
                }
            }
        }

        private void HandleServerStarted()
        {
        }

        private void HandleClientStarted()
        {
        }
    }
}
