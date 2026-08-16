using System;
using Core;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInput;
        [SerializeField] private TMP_InputField roomCodeInput;
    
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;

        private string Nickname => nicknameInput.text;
        private string RoomCode => roomCodeInput.text;

        private void Start()
        {
            nicknameInput.text = LocalDataManager.Instance.Nickname;
        }

        private void OnEnable()
        {
            quickMatchButton.onClick.AddListener(HandleQuickMatchButton);
            createButton.onClick.AddListener(HandleCreateButton);
            joinButton.onClick.AddListener(HandleJoinButton);
        }

        private void OnDisable()
        {
            quickMatchButton.onClick.RemoveListener(HandleQuickMatchButton);
            createButton.onClick.RemoveListener(HandleCreateButton);
            joinButton.onClick.RemoveListener(HandleJoinButton);
        }

        private async void HandleQuickMatchButton()
        {
            SetUIInteractable(false);
            
            SetNickname(Nickname);
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            var result = await NetworkManager.Instance.Connection.MatchQuick(sceneIndex);
            
            if (result is { Ok: true })
            {
                string roomCode = RoomManager.Instance.RoomCode;
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

        private async void HandleCreateButton()
        {
            SetUIInteractable(false);
            
            SetNickname(Nickname);
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            var result = await NetworkManager.Instance.Connection.CreateRoom(sceneIndex);
            
            if (result is { Ok: true })
            {
                string roomCode = RoomManager.Instance.RoomCode;
                bool isPrivate = RoomManager.Instance.IsPrivate;
                
                Debug.Log($"[{roomCode}] 방 생성 성공! ({(isPrivate ? "비공개" : "공개")})");
            }
            else
            {
                SetUIInteractable(true);
                Debug.LogError($"방 생성 실패: {result?.ShutdownReason}");
            }
        }
    
        private async void HandleJoinButton()
        {
            SetUIInteractable(false);
            
            SetNickname(Nickname);
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            var result = await NetworkManager.Instance.Connection.JoinRoom(RoomCode, sceneIndex);
            
            if (result.Ok)
            {
                Debug.Log($"[{RoomCode}] 방 참가 성공!");
            }
            else
            {
                SetUIInteractable(true);
                Debug.LogError($"방 참가 실패: {result.ShutdownReason}");
            }
        }

        private void SetNickname(string nickname)
        {
            LocalDataManager.Instance.Nickname = nickname;
        }

        private void SetUIInteractable(bool interactable)
        {
            SetButtonInteractable(interactable);
            SetInputInteractable(interactable);
        }
        
        private void SetButtonInteractable(bool interactable)
        {
            quickMatchButton.interactable = interactable;
            createButton.interactable = interactable;
            joinButton.interactable = interactable;
        }
        
        private void SetInputInteractable(bool interactable)
        {
            nicknameInput.interactable = interactable;
            roomCodeInput.interactable = interactable;
        }
    }
}