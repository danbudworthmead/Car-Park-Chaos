using System.Threading.Tasks;
using IngameDebugConsole;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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
        
        private void Start()
        {
            Instance = this;
            DebugLogConsole.AddCommandInstance("createRelay", "Create a relay", "CreateRelay", this);
            DebugLogConsole.AddCommandInstance("joinRelay", "Join a relay", "JoinRelay", this);
        }

        public async Task<string> CreateRelay()
        {
            var code = "ERR";
            
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(3);
                code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                RelayServerData = new RelayServerData(allocation, "dtls");
                AmHost = true;
                SceneManager.LoadScene("Game");
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
                SceneManager.LoadScene("Game");
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
