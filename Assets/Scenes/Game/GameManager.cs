using _Game.Camera;
using Scenes.Online;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Scenes.Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Transform wideCamera;
        [SerializeField] private CameraRig cameraRig;
        
        private void Start()
        {
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

        private void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"Client connected: {clientId}");

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                var player = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
                
                // move the player car to the correct position
                player.transform.position = new Vector3(clientId * 5f, 0, 0);
                player.name = $"Player {clientId}";
                
                // we are the local client
                // enable the camera rig and disable the wide camera
                wideCamera.gameObject.SetActive(false);
                cameraRig.SetTarget(player);
                cameraRig.gameObject.SetActive(true);
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
