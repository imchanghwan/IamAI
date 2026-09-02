using System;
using Core;
using Game;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInputField;
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private Button createButton;
        [SerializeField] private TMP_InputField roomCodeInputField;
        [SerializeField] private Button joinButton;
        
        private string Nickname
        {
            get => nicknameInputField.text;
            set => nicknameInputField.text = value;
        }

        private string RoomCode => roomCodeInputField.text;

        private void Start()
        {
            Nickname = PlayerPrefs.GetString(PrefKeys.Nickname, string.Empty);
        }

        private void OnEnable()
        {
            quickMatchButton.onClick.AddListener(OnQuickMatchButtonClick);
            createButton.onClick.AddListener(OnCreateButtonClick);
            joinButton.onClick.AddListener(OnJoinButtonClick);
        }

        private void OnDisable()
        {
            quickMatchButton.onClick.RemoveListener(OnQuickMatchButtonClick);
            createButton.onClick.RemoveListener(OnCreateButtonClick);
            joinButton.onClick.RemoveListener(OnJoinButtonClick);
        }

        private async void OnQuickMatchButtonClick()
        {
            SetUIInteractable(false);
            GameManager.Instance.Nickname = Nickname;
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            var result = await SessionManager.Instance.MatchQuick(sceneIndex);
            
            if (result is { Ok: true })
            {
                string roomCode = SessionManager.Instance.RoomCode;
                Debug.Log($"[QuickJoin] 참가 성공! 방 코드: {roomCode}");
                // UI 전환, 씬 로드 등
            }
            else
            {
                SetUIInteractable(true);
                Debug.LogError($"매칭 실패: {result?.ShutdownReason}");
                // 오류 UI 표시 등
            }
        }

        private async void OnCreateButtonClick()
        {
            SetUIInteractable(false);
            GameManager.Instance.Nickname = Nickname;
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            var result = await SessionManager.Instance.CreateRoom(sceneIndex);
            
            if (result is { Ok: true })
            {
                string roomCode = SessionManager.Instance.RoomCode;
                bool isPrivate = SessionManager.Instance.IsPrivate;
                
                Debug.Log($"[{roomCode}] 방 생성 성공! ({(isPrivate ? "비공개" : "공개")})");
            }
            else
            {
                SetUIInteractable(true);
                Debug.LogError($"방 생성 실패: {result?.ShutdownReason}");
            }
        }

        private async void OnJoinButtonClick()
        {
            SetUIInteractable(false);
            GameManager.Instance.Nickname = Nickname;
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            var result = await SessionManager.Instance.JoinRoom(RoomCode, sceneIndex);

            if (result is { Ok: true })
            {
                string roomCode = SessionManager.Instance.RoomCode;
                Debug.Log($"[{roomCode}] 방 참가 성공!");
            }
            else
            {
                SetUIInteractable(true);
                Debug.LogError($"방 참가 실패: {result.ShutdownReason}");
            }
        }

        private void SetUIInteractable(bool interactable)
        {
            nicknameInputField.interactable = interactable;
            roomCodeInputField.interactable = interactable;
            quickMatchButton.interactable = interactable;
            joinButton.interactable = interactable;
            createButton.interactable = interactable;
        }
    }
}
