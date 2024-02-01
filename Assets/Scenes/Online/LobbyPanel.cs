using UnityEngine;

namespace Scenes.Online
{
    public class LobbyPanel : MonoBehaviour
    {
        [SerializeField] private GameObject browsePanel;
        [SerializeField] private GameObject createPanel;
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject tabsPanel;
        
        public async void ShowBrowsePanel()
        {
            browsePanel.SetActive(true);
            createPanel.SetActive(false);
            lobbyPanel.SetActive(false);
            tabsPanel.SetActive(true);

            var browser = browsePanel.GetComponent<LobbyBrowser>();
            await browser.RefreshLobbies();
        }
        
        public void ShowCreatePanel()
        {
            createPanel.SetActive(true);
            browsePanel.SetActive(false);
            lobbyPanel.SetActive(false);
            tabsPanel.SetActive(true);
        }

        public void JoinedLobby()
        {
            browsePanel.SetActive(false);
            createPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            tabsPanel.SetActive(false);
        }
    }
}
