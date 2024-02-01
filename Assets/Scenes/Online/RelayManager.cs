using System.Threading.Tasks;
using IngameDebugConsole;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using UnityEngine;

namespace Scenes.Online
{
    public class RelayManager : MonoBehaviour
    {
        public static RelayManager Instance;
        
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
                Debug.Log(code);

                var relayServerData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartHost();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }

            return code;
        }

        public async void JoinRelay(string joinCode)
        {
            try
            {
                var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                
                var relayServerData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
