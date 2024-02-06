using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace Scenes.Online.LobbyViewer
{
    public class LobbyBrowser : MonoBehaviour
    {
        [SerializeField] private Transform lobbyListParent;
        [SerializeField] private GameObject lobbyPanelPrefab;

        private void Update()
        {
            if (AuthenticationService.Instance.IsAuthorized && !IsInvoking(nameof(RefreshLobbies)))
            {
                InvokeRepeating(nameof(RefreshLobbies), 0f, 3f);
            }
        }

        public async Task RefreshLobbies()
        {
            if (gameObject.activeSelf == false)
            {
                return;
            }
            
            var lobbies = await LobbyManager.Instance.ListLobbies();
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (lobbies == null)
            {
                // destroy all panels
                foreach (Transform child in lobbyListParent)
                {
                    Destroy(child.gameObject);
                }
                return;
            }
            
            // update panels that already exist
            foreach (Transform child in lobbyListParent)
            {
                var lobbyPanel = child.GetComponent<LobbyDetailsPanel>();
                if (lobbyPanel != null)
                {
                    var lobby = lobbies.Find(l => l.Id == lobbyPanel.name);
                    if (lobby != null)
                    {
                        lobbyPanel.Set(lobby);
                    }
                }
            }
            
            // create new panels for new lobbies
            foreach (var lobby in lobbies)
            {
                if (lobbyListParent.Find(lobby.Id) == null)
                {
                    var lobbyPanel = Instantiate(lobbyPanelPrefab, lobbyListParent).GetComponent<LobbyDetailsPanel>();
                    lobbyPanel.Set(lobby);
                }
            }
            
            // destroy panels for lobbies that no longer exist
            foreach (Transform child in lobbyListParent)
            {
                var lobbyPanel = child.GetComponent<LobbyDetailsPanel>();
                if (lobbyPanel != null)
                {
                    var lobby = lobbies.Find(l => l.Id == lobbyPanel.name);
                    if (lobby == null)
                    {
                        Destroy(lobbyPanel.gameObject);
                    }
                }
            }
        }
    }
}
