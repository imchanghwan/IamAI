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

        [Header("안내 문구 (선택)")]
        [Tooltip("연결하면 실패 사유가 화면에 표시된다. 비워두면 로그로만 남는다.")]
        [SerializeField] private TMP_Text statusText;
        
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
            bool entered = false;

            try
            {
                GameManager.Instance.Nickname = NicknameText;
                int sceneIndex = SceneName.GetIndex(SceneName.Room);
                var result = await SessionManager.Instance.MatchQuick(sceneIndex);

                entered = result is { Ok: true };
                if (entered)
                    Debug.Log($"[QuickJoin] 참가 성공! 방 코드: {SessionManager.Instance.RoomCode}");
                else
                    ShowMessage(ShutdownReasonMessage.Get(result));
            }
            catch (Exception e)
            {
                // async void에서 예외가 새면 UI가 잠긴 채로 영구히 멈춘다.
                Debug.LogException(e);
                ShowMessage(ShutdownReasonMessage.Unknown);
            }
            finally
            {
                // 성공하면 씬이 전환되므로 잠금을 유지한다. 실패·예외일 때만 되돌린다.
                if (!entered) SetUIInteractable(true);
            }
        }

        private async void OnCreateButtonClick()
        {
            SetUIInteractable(false);
            bool entered = false;

            try
            {
                GameManager.Instance.Nickname = NicknameText;
                int sceneIndex = SceneName.GetIndex(SceneName.Room);
                var result = await SessionManager.Instance.CreateRoom(sceneIndex);

                entered = result is { Ok: true };
                if (entered)
                {
                    var roomCode = SessionManager.Instance.RoomCode;
                    var isPrivate = SessionManager.Instance.IsPrivate;
                    Debug.Log($"[{roomCode}] 방 생성 성공! ({(isPrivate ? "비공개" : "공개")})");
                }
                else
                {
                    ShowMessage(ShutdownReasonMessage.Get(result));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ShowMessage(ShutdownReasonMessage.Unknown);
            }
            finally
            {
                if (!entered) SetUIInteractable(true);
            }
        }

        private async void OnJoinButtonClick()
        {
            // 빈 코드를 그대로 넘기면 Fusion이 "아무 공개방에나 참가"로 처리해
            // 의도하지 않은 방에 들어간다. UI를 잠그기 전에 먼저 막는다.
            if (!SessionManager.IsValidRoomCode(RoomCode))
            {
                ShowMessage($"방 코드는 숫자 {SessionManager.RoomCodeLength}자리여야 합니다.");
                return;
            }

            SetUIInteractable(false);
            bool entered = false;

            try
            {
                GameManager.Instance.Nickname = NicknameText;
                int sceneIndex = SceneName.GetIndex(SceneName.Room);
                var result = await SessionManager.Instance.JoinRoom(RoomCode, sceneIndex);

                entered = result is { Ok: true };
                if (entered)
                    Debug.Log($"[{SessionManager.Instance.RoomCode}] 방 참가 성공!");
                else
                    ShowMessage(ShutdownReasonMessage.Get(result));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ShowMessage(ShutdownReasonMessage.Unknown);
            }
            finally
            {
                if (!entered) SetUIInteractable(true);
            }
        }

        /// <summary>
        /// 사용자에게 보여줄 안내 문구. statusText가 연결돼 있으면 화면에도 표시한다.
        /// </summary>
        private void ShowMessage(string message)
        {
            Debug.LogWarning(message);

            if (statusText != null)
                statusText.text = message;
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
