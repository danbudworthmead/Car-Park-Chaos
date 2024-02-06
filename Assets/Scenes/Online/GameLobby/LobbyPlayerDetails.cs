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
        
        private float _readyCooldown = 0f;

        private void Start()
        {
            _readyCooldown = 1f;
            toggle.interactable = LobbyManager.Instance.MyId == name;
        }

        private void FixedUpdate()
        {
            if (name == LobbyManager.Instance.MyId
                && _readyCooldown > 0)
            {
                _readyCooldown -= Time.fixedDeltaTime;
                if (_readyCooldown <= 0)
                {
                    toggle.interactable = LobbyManager.Instance.MyId == name;
                }
            }
        }

        public void Set(Player player)
        {
            if (name == LobbyManager.Instance.MyId) return;
            
            playerName.text = player.Data["PlayerName"].Value;
            toggle.isOn = player.Data["Ready"].Value == "true";
            name = player.Id;
        }

        public async void OnReadyToggle()
        {
            if (name == LobbyManager.Instance.MyId)
            {
                await LobbyManager.Instance.SetReady(toggle.isOn);
                toggle.interactable = false;
                _readyCooldown = 1f;
            }
        }
    }
}
