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
    public class Room : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomCode;
        [SerializeField] private Button leaveButton;
        
        [SerializeField] private RectTransform uiContainer;
        [SerializeField] private NetworkObject playerCardPrefab;
        
        private NetworkRunner _runner;
        private readonly Dictionary<PlayerRef, NetworkObject> _players = new();

        private void Start()
        {
            _runner = NetworkManager.Instance.CreateRunner();
    
            UpdateRoomCode();
        }

        private void OnEnable()
        {
            leaveButton.onClick.AddListener(HandleLeaveButton);
            
            GlobalEventHub.Instance.Network.OnPlayerJoinedEvent += AddPlayerUI;
            GlobalEventHub.Instance.Network.OnPlayerLeftEvent += RemovePlayerUI;
            GlobalEventHub.Instance.Network.OnShutdownEvent += LeaveScene;
        }

        private void OnDisable()
        {
            leaveButton.onClick.RemoveListener(HandleLeaveButton);
            
            GlobalEventHub.Instance.Network.OnPlayerJoinedEvent -= AddPlayerUI;
            GlobalEventHub.Instance.Network.OnPlayerLeftEvent -= RemovePlayerUI;
            GlobalEventHub.Instance.Network.OnShutdownEvent -= LeaveScene;
        }

        private void UpdateRoomCode()
        {
            if (_runner == null || _runner.SessionInfo == null) 
            {
                roomCode.text = "연결 중..."; // 또는 string.Empty
                return;
            }

            if (_runner.SessionInfo.Properties.TryGetValue(PrefKeys.RoomCode, out var prop))
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

        private void LeaveScene(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            SceneManager.LoadScene(SceneName.Lobby);
        }
    }
}
