using System;
using System.Collections.Generic;
using Fusion;
using IamAI.Network;
using Photon.Client.StructWrapping;
using IamAI.Player;
using IamAI.Room;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using IamAI.Utils;

namespace IamAI.UI
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

            startButton.enabled = _runner.IsServer;
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
            if (!_runner.IsServer) return;

            if (SessionManager.Instance.PlayerCount < SessionManager.MinPlayers)
            {
                Debug.LogError("Player count is too low");
                // return;
            }

            // 게임이 시작되면 세션을 닫아 빠른 매칭으로 난입하는 것을 막는다.
            // 방으로 복귀하는 흐름이 생기면 그 시점에 SetJoinable(true)로 되돌려야 한다.
            SessionManager.Instance.SetJoinable(false);

            var sceneIndex = SceneName.GetIndex(SceneName.Game);
            _runner.LoadScene(SceneRef.FromIndex(sceneIndex));
        }
    }
}
