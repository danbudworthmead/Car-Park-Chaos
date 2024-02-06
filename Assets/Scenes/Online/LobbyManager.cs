using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IngameDebugConsole;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scenes.Online
{
    public class LobbyManager : MonoBehaviour
    {
        private const string KeyStartGame = "StartGame_RelayCode";
        private const string KeyCountdown = "StartGame_Countdown";

        private Lobby _joinedLobby;
        private float _heartbeatTimer;
        private string _playerName;
        public static LobbyManager Instance { get; private set; }
        public string MyId => AuthenticationService.Instance.PlayerId;
        
        private int _countdownTimer = 0;
        private bool _hasStarted = false;
        private bool _startedJoiningRelay = false;
        private float _timeSinceLastRefresh = 0f;

        private void Awake()
        {
            Instance = this;
        }

        private async void Start()
        {
            await UnityServices.InitializeAsync();
            
            if (AuthenticationService.Instance.IsSignedIn) return;
            
            _playerName = "BadDriver" + Random.Range(0, 999);
            
            DebugLogConsole.AddCommandInstance("createLobby", "Create a lobby", "CreateLobby", this);
            DebugLogConsole.AddCommandInstance("listLobbies", "List lobbies", "ListLobbies", this);
            DebugLogConsole.AddCommandInstance("quickJoinLobby", "Quick join a lobby", "QuickJoinLobby", this);            
            DebugLogConsole.AddCommandInstance("startLobby", "Start a lobby", "StartLobby", this);
            
            Debug.Log(_playerName);

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
            };
            AuthenticationService.Instance.SwitchProfile($"Clone_{_playerName}_Profile");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        private void Update()
        {
            HandleLobbyHeartbeat();
            
            if (_joinedLobby == null) return;
            

            // check the lobby hasn't started
            if (AmHost() && !HasStarted())
            {
                // check if all players are ready
                if (AreAllPlayersReady() && !IsInvoking(nameof(Countdown)))
                {
                    _countdownTimer = 5;
                    InvokeRepeating(nameof(Countdown), 1f, 1f);
                }
            }
        }
        
        private bool HasStarted()
        {
            return _hasStarted;
        }

        private async void Countdown()
        {
            if (HasStarted())
            {
                CancelInvoke(nameof(Countdown));
                _countdownTimer = 0;
                return;
            }
            
            if (!AreAllPlayersReady())
            {
                CancelInvoke(nameof(Countdown));
                _countdownTimer = 5;
                return;
            }
            
            // update lobby countdown
            try
            {
                _joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {
                            KeyCountdown,
                            new DataObject(DataObject.VisibilityOptions.Member, _countdownTimer.ToString())
                        }
                    }
                });
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
            
            if (_countdownTimer == 0)
            {
                StartLobby();
                CancelInvoke(nameof(Countdown));
                return;
            }
            
            _countdownTimer -= 1;
        }

        private bool AreAllPlayersReady()
        {
            return _joinedLobby.Players.All(player => player.Data["Ready"].Value == "true");
        }

        private bool AmHost()
        {
            return _joinedLobby.HostId == MyId;
        }

        private void HandleLobbyHeartbeat()
        {
            if (_joinedLobby == null) return;
            _heartbeatTimer += Time.deltaTime;
            
            if (_heartbeatTimer > 15f)
            {
                _heartbeatTimer = 0f;
                try
                {
                    LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public async Task CreateLobby(string lobbyName)
        {
            var maxPlayers = 4;
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { KeyStartGame, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };
            
            try
            {
                var lobby =  await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options);
                
                _joinedLobby = lobby;
                
                Debug.Log($"Created lobby: {lobby.Id}");
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async Task<List<Lobby>> ListLobbies()
        {
            try
            {
                var response = await Lobbies.Instance.QueryLobbiesAsync();
                return response.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
            }

            return null;
        }

        private async void QuickJoinLobby()
        {
            var options = new QuickJoinLobbyOptions()
            {
                Player = GetPlayer()
            };

            try
            {
                await Lobbies.Instance.QuickJoinLobbyAsync(options);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        private Player GetPlayer()
        {
            return new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) },
                    { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                }
            };
        }

        private async void StartLobby()
        {
            _hasStarted = true;
            if (AmHost())
            {
                try
                {
                    _startedJoiningRelay = true;
                    var code = await RelayManager.Instance.CreateRelay(_joinedLobby.Players);
                    
                    var lobby = await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            { KeyStartGame, new DataObject(DataObject.VisibilityOptions.Member, code) }
                        }
                    });
                    
                    _joinedLobby = lobby;
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public List<Player> GetPlayers()
        {
            return _joinedLobby.Players;
        }

        public string GetCountdown()
        {
            if (_joinedLobby == null || !_joinedLobby.Data.ContainsKey(KeyCountdown)) return string.Empty;
            return _joinedLobby.Data[KeyCountdown].Value;
        }

        public async Task<bool> JoinLobby(string id)
        {
            try
            {
                _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, new JoinLobbyByIdOptions
                {
                    Player = GetPlayer()
                });
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }

        public async Task RefreshLobby()
        {
            if (Time.time  - _timeSinceLastRefresh < 1f) return;
            _timeSinceLastRefresh = Time.time;
            _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
            if (_joinedLobby.Data[KeyStartGame].Value != "0")
            {
                await JoinRelay();
            }
        }

        private async Task JoinRelay()
        {
            if (!AmHost() && !_startedJoiningRelay)
            {
                _startedJoiningRelay = true;
                await RelayManager.Instance.JoinRelay(_joinedLobby.Data[KeyStartGame].Value);
            }
        }

        public async Task SetReady(bool toggleIsOn)
        {
            try
            {
                await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, MyId, new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, toggleIsOn.ToString().ToLower()) }
                    }
                });
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
