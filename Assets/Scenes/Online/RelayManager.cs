using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Online
{
    public class RelayManager : MonoBehaviour
    {
        public static RelayManager Instance;

        public RelayServerData RelayServerData { get; private set; }
        public bool AmHost { get; private set; }
        
        public List<Player> PlayersInLobby { get; private set; } = new();
        
        private void Start()
        {
            Instance = this;
        }

        public async Task<string> CreateRelay(List<Player> playersInLobby)
        {
            var code = "ERR";
            
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(3);
                code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                RelayServerData = new RelayServerData(allocation, "dtls");
                AmHost = true;
                PlayersInLobby = playersInLobby;
                SceneManager.LoadScene("Level001");
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }

            return code;
        }

        public async Task JoinRelay(string joinCode)
        {
            try
            {
                var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData = new RelayServerData(allocation, "dtls");
                AmHost = false;
                SceneManager.LoadScene("Level001");
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
