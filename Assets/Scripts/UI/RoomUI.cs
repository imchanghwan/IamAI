using System;
using System.Collections.Generic;
using Fusion;
using Network;
using Photon.Client.StructWrapping;
using Player;
using Room;
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
        
        private NetworkRunner _runner;

        private void Awake()
        {
            _runner = NetworkManager.Instance.Runner;
        }

        private void Start()
        {
            roomCode.text = SessionManager.Instance.RoomCode;
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
        
        private async void OnLeaveButtonClick()
        {
            await NetworkManager.Instance.RemoveRunner();
        }

        private void OnStartButtonClick()
        {
            var sceneIndex = SceneName.GetIndex(SceneName.Game);
            
            if (!_runner.IsServer) return;
            
            _runner.LoadScene(SceneRef.FromIndex(sceneIndex));
        }
    }
}
