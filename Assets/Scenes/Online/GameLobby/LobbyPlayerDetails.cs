using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Online.GameLobby
{
    public class LobbyPlayerDetails : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private Toggle toggle;
        
        public void Set(Player player)
        {
            playerName.text = player.Data["PlayerName"].Value;
            toggle.isOn = player.Data["Ready"].Value == "true";
            name = player.Id;
        }
    }
}
