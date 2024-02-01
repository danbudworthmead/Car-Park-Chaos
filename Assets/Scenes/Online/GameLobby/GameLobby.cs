using System.Threading.Tasks;
using UnityEngine;

namespace Scenes.Online.GameLobby
{
    public class GameLobby : MonoBehaviour
    {
        [SerializeField] private Transform listParent;
        [SerializeField] private GameObject playerDetailsPrefab;
        
        private float heartbeatTimer = 0f;

        private void OnEnable()
        {
            RefreshLobby();
        }

        private async void FixedUpdate()
        {
            await HandleHeartbeat();
        }

        private async Task HandleHeartbeat()
        {
            heartbeatTimer += Time.fixedDeltaTime;
            if (heartbeatTimer >= 1.1f)
            {
                heartbeatTimer = 0f;
                await RefreshLobby();
            }
        }

        public async Task RefreshLobby()
        {
            await LobbyManager.Instance.RefreshLobby();
            
            var players = LobbyManager.Instance.GetPlayers();
            
            // update panels that already exist
            foreach (Transform child in listParent)
            {
                var playerDetails = child.GetComponent<LobbyPlayerDetails>();
                if (playerDetails != null)
                {
                    var player = players.Find(p => p.Id == playerDetails.name);
                    if (player != null)
                    {
                        playerDetails.Set(player);
                    }
                }
            }
            
            // create new panels for new players
            foreach (var player in players)
            {
                if (listParent.Find(player.Id) == null)
                {
                    var playerDetails = Instantiate(playerDetailsPrefab, listParent).GetComponent<LobbyPlayerDetails>();
                    playerDetails.Set(player);
                }
            }
            
            // remove panels for players that have left
            foreach (Transform child in listParent)
            {
                var playerDetails = child.GetComponent<LobbyPlayerDetails>();
                if (playerDetails != null)
                {
                    if (players.Find(p => p.Id == playerDetails.name) == null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
    }
}
