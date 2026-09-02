using System.Collections.Generic;
using Core;
using Fusion;
using Old.Core;
using Old.Network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace Old.UI
{
    public class RoomUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomCode;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button startButton;
        
        [SerializeField] private NetworkObject playerCardPrefab;
        
        private readonly Dictionary<PlayerRef, NetworkObject> _players = new();

        private void Start()
        {
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
            roomCode.text = Old.Core.RoomManager.Instance.RoomCode;
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
