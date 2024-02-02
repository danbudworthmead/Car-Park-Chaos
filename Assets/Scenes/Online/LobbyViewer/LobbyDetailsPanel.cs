using System.Globalization;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Scenes.Online
{
    public class LobbyDetailsPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyName;
        [SerializeField] private TMP_Text lobbyCode;
        [SerializeField] private TMP_Text lobbyPlayers;

        public void Set(Lobby lobby)
        {
            lobbyName.text = lobby.Name;
            lobbyCode.text = lobby.LobbyCode;
            lobbyPlayers.text = $"{lobby.Players.Count.ToString(CultureInfo.InvariantCulture)}/{lobby.MaxPlayers.ToString(CultureInfo.InvariantCulture)}";
            name = lobby.Id;
        }

        public async void Join()
        {
            if (await LobbyManager.Instance.JoinLobby(name))
            {
                FindObjectOfType<LobbyPanel>().JoinedLobby();
            }
        }
    }
}
