using TMPro;
using UnityEngine;

namespace Scenes.Online
{
    public class LobbyCreator : MonoBehaviour
    {
        [SerializeField] private TMP_InputField lobbyName;
        [SerializeField] private LobbyPanel lobbyPanel;
        
        public async void CreateLobby()
        {
            await LobbyManager.Instance.CreateLobby(lobbyName.text);
            lobbyPanel.JoinedLobby();
        }
    }
}
