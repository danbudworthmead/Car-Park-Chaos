using Unity.Netcode;
using UnityEngine;

namespace _Game.UI
{
    public class NetworkManagerUI : MonoBehaviour
    {
        public void StartAsServer()
        {
            NetworkManager.Singleton.StartServer();
        }

        public void StartAsHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        public void StartAsClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
