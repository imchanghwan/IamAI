using System;
using System.Collections.Generic;
using Core;
using Fusion;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class RoomUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomCode;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button startButton;
        
        [SerializeField] private RectTransform uiContainer;
        [SerializeField] private NetworkObject playerCardPrefab;
        
        private SessionInfo _roomInfo;
        private readonly Dictionary<PlayerRef, NetworkObject> _players = new();

        private void Start()
        {
            _roomInfo = GameManager.Instance.RoomInfo;
    
            UpdateRoomCode();
        }

        private void OnEnable()
        {
            leaveButton.onClick.AddListener(HandleLeaveButton);
            startButton.onClick.AddListener(HandleStartButton);
            
            GlobalEventHub.Instance.Network.OnPlayerJoinedEvent += AddPlayerUI;
            GlobalEventHub.Instance.Network.OnPlayerLeftEvent += RemovePlayerUI;
            GlobalEventHub.Instance.Network.OnShutdownEvent += LeaveScene;
        }

        private void OnDisable()
        {
            leaveButton.onClick.RemoveListener(HandleLeaveButton);
            startButton.onClick.RemoveListener(HandleStartButton);
            
            GlobalEventHub.Instance.Network.OnPlayerJoinedEvent -= AddPlayerUI;
            GlobalEventHub.Instance.Network.OnPlayerLeftEvent -= RemovePlayerUI;
            GlobalEventHub.Instance.Network.OnShutdownEvent -= LeaveScene;
        }

        private void UpdateRoomCode()
        {
            if (_roomInfo == null) 
            {
                roomCode.text = "연결 중..."; // 또는 string.Empty
                return;
            }

            if (_roomInfo.Properties.TryGetValue(PrefKeys.RoomCode, out var prop))
            {
                roomCode.text = (string)prop; 
            }
        }
        
        private void AddPlayerUI(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer || _players.ContainsKey(player)) return;
            _players[player] = runner.Spawn(playerCardPrefab, inputAuthority: player);
        }
        
        private void RemovePlayerUI(NetworkRunner runner, PlayerRef player)
        {
            if (!_players.TryGetValue(player, out var obj)) return;
            runner.Despawn(obj);
            _players.Remove(player);
        }

        private async void HandleLeaveButton()
        {
            await NetworkManager.Instance.Connection.Shutdown();
        }

        private void HandleStartButton()
        {
            var sceneIndex = SceneName.GetIndex(SceneName.Game);
            NetworkManager.Instance.Connection.LoadScene(sceneIndex);
        }

        private void LeaveScene(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            SceneManager.LoadScene(SceneName.Lobby);
        }
    }
}
