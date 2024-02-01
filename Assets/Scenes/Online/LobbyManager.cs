using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IngameDebugConsole;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Scenes.Online
{
    public class LobbyManager : MonoBehaviour
    {
        private const string KeyStartGame = "StartGame_RelayCode";

        private Lobby _joinedLobby;
        private float _heartbeatTimer;
        private string _playerName;
        public static LobbyManager Instance { get; private set; }
        public string MyId => AuthenticationService.Instance.PlayerId;

        private void Awake()
        {
            Instance = this;
        }

        private async void Start()
        {
            _playerName = "BadDriver" + UnityEngine.Random.Range(0, 999);
            
            DebugLogConsole.AddCommandInstance("createLobby", "Create a lobby", "CreateLobby", this);
            DebugLogConsole.AddCommandInstance("listLobbies", "List lobbies", "ListLobbies", this);
            DebugLogConsole.AddCommandInstance("quickJoinLobby", "Quick join a lobby", "QuickJoinLobby", this);            
            DebugLogConsole.AddCommandInstance("startLobby", "Start a lobby", "StartLobby", this);
            
            Debug.Log(_playerName);
                        
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        private void Update()
        {
            HandleLobbyHeartbeat();
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
                Debug.Log($"Lobbies: {response.Results.Count}");

                foreach (var lobby in response.Results)
                {
                    Debug.Log($"Lobby: {lobby.Id} - {lobby.Name} - {lobby.Players.Count}/{lobby.MaxPlayers}");
                }

                return response.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
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
            if (IsLobbyHost())
            {
                try
                {
                    var code = await RelayManager.Instance.CreateRelay();
                    
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

        private bool IsLobbyHost()
        {
            return _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        public List<Player> GetPlayers()
        {
            return _joinedLobby.Players;
        }

        public async Task JoinLobby(string id)
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            });
        }

        public async Task RefreshLobby()
        {
            _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
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
