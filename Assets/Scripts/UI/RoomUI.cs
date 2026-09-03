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
        [Header("Network")]
        [SerializeField] private NetworkObject playerDataPrefab;
        
        [Header("UI Elements")]
        [SerializeField] private TMP_Text roomCode;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button startButton;
        
        private NetworkEvents _networkEvents;

        private void Awake()
        {
            _networkEvents = NetworkManager.Instance.Events;
        }

        private void Start()
        {
            UpdateRoomCode();
        }

        private void OnEnable()
        {
            leaveButton.onClick.AddListener(OnLeaveButtonClick);
            startButton.onClick.AddListener(OnStartButtonClick);
            
            _networkEvents.PlayerJoined.AddListener(OnPlayerJoined);
            _networkEvents.PlayerLeft.AddListener(OnPlayerLeft);
            _networkEvents.OnShutdown.AddListener(OnShutDown);
        }

        private void OnDisable()
        {
            leaveButton.onClick.RemoveListener(OnLeaveButtonClick);
            startButton.onClick.RemoveListener(OnStartButtonClick);
            
            _networkEvents.PlayerJoined.RemoveListener(OnPlayerJoined);
            _networkEvents.PlayerLeft.RemoveListener(OnPlayerLeft);
            _networkEvents.OnShutdown.RemoveListener(OnShutDown);
        }

        private void UpdateRoomCode()
        {
            roomCode.text = SessionManager.Instance.RoomCode;
        }
        
        private async void OnLeaveButtonClick()
        {
            var runner = NetworkManager.Instance.Runner;
            if (runner == null || !runner.IsRunning) return;
            
            await NetworkManager.Instance.RemoveRunner();
        }

        private void OnStartButtonClick()
        {
            var sceneIndex = SceneName.GetIndex(SceneName.Game);
            var runner = NetworkManager.Instance.Runner;
            
            if (!runner.IsServer) return;
            
            runner.LoadScene(SceneRef.FromIndex(sceneIndex));
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Joined");
            // Host만 Spawn
            if (!runner.IsServer) return;
            var obj = runner.Spawn(playerDataPrefab, inputAuthority: player);
            runner.SetPlayerObject(player, obj);
        }

        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.TryGetPlayerObject(player, out var obj)) return;
            runner.Despawn(obj);
        }

        private void OnShutDown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            SceneManager.LoadScene(SceneName.Lobby);
        }
    }
}
