using System;
using IamAI.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IamAI.Utils;

namespace IamAI.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInputField;
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private Button createButton;
        [SerializeField] private TMP_InputField roomCodeInputField;
        [SerializeField] private Button joinButton;
        
        private string NicknameText
        {
            get => nicknameInputField.text;
            set => nicknameInputField.text = value;
        }

        private string RoomCode => roomCodeInputField.text;

        private void Start()
        {
            NicknameText = PlayerPrefs.GetString(PrefKeys.Nickname, string.Empty);

            // 입력 단계에서 형식을 강제한다. 인스펙터 설정과 무관하게 항상 적용되도록 코드에서 지정.
            roomCodeInputField.contentType    = TMP_InputField.ContentType.IntegerNumber;
            roomCodeInputField.characterLimit = SessionManager.RoomCodeLength;
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
            GameManager.Instance.Nickname = NicknameText;
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
            GameManager.Instance.Nickname = NicknameText;
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
            // 빈 코드를 그대로 넘기면 Fusion이 "아무 공개방에나 참가"로 처리해
            // 의도하지 않은 방에 들어간다. UI를 잠그기 전에 먼저 막는다.
            if (!SessionManager.IsValidRoomCode(RoomCode))
            {
                Debug.LogError($"방 코드는 숫자 {SessionManager.RoomCodeLength}자리여야 합니다.");
                return;
            }

            SetUIInteractable(false);
            GameManager.Instance.Nickname = NicknameText;
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
