using System.Threading.Tasks;
using Scenes.Online;
using UnityEngine;

public class LobbyBrowser : MonoBehaviour
{
    [SerializeField] private Transform lobbyListParent;
    [SerializeField] private GameObject lobbyPanelPrefab;

    public async Task RefreshLobbies()
    {
        // destroy all children of lobbyPanelPrefab
        foreach (Transform child in lobbyListParent)
        {
            Destroy(child.gameObject);
        }
        
        var lobbies = await LobbyManager.Instance.ListLobbies();
        if (lobbies == null) return;
        foreach (var lobby in lobbies)
        {
            var lobbyPanel = Instantiate(lobbyPanelPrefab, lobbyListParent).GetComponent<LobbyDetailsPanel>();
            lobbyPanel.Set(lobby);
        }
    }
}
