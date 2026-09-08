using System;
using System.Collections.Generic;
using Fusion;
using Network;
using Photon.Client.StructWrapping;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class RoomUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text roomCode;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button startButton;
        
        private void Start()
        {
            UpdateRoomCode();
        }

        private void OnEnable()
        {
            leaveButton.onClick.AddListener(OnLeaveButtonClick);
            startButton.onClick.AddListener(OnStartButtonClick);
        }

        private void OnDisable()
        {
            leaveButton.onClick.RemoveListener(OnLeaveButtonClick);
            startButton.onClick.RemoveListener(OnStartButtonClick);
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
    }
}
