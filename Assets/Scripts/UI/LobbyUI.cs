using System;
using System.Threading.Tasks;
using Fusion;
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

            // 세션이 끊겨 로비로 돌아온 경우. 사유는 끊긴 씬에서 담아 보낸다.
            if (GameManager.Instance.TryTakePendingMessage(out var pendingMessage))
                ShowMessage(pendingMessage);
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
            await EnterRoom(
                sceneIndex => SessionManager.Instance.MatchQuick(sceneIndex),
                () => $"[QuickJoin] 참가 성공! 방 코드: {SessionManager.Instance.RoomCode}");
        }

        private async void OnCreateButtonClick()
        {
            await EnterRoom(
                sceneIndex => SessionManager.Instance.CreateRoom(sceneIndex),
                () =>
                {
                    var isPrivate = SessionManager.Instance.IsPrivate;
                    return $"[{SessionManager.Instance.RoomCode}] 방 생성 성공! " +
                           $"({(isPrivate ? "비공개" : "공개")})";
                });
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

            // RoomCode는 요청 시점이 아니라 지금 값을 캡처한다.
            var roomCode = RoomCode;
            await EnterRoom(
                sceneIndex => SessionManager.Instance.JoinRoom(roomCode, sceneIndex),
                () => $"[{SessionManager.Instance.RoomCode}] 방 참가 성공!");
        }

        /// <summary>
        /// 세 가지 세션 진입 경로(빠른 매칭·방 생성·코드 참가)의 공통 흐름.
        /// UI 잠금 → 요청 → 결과 처리 순서로 진행하며, 실패·예외일 때만 잠금을 되돌린다.
        /// </summary>
        /// <param name="request">방 씬 인덱스를 받아 세션 진입을 수행하는 요청.</param>
        /// <param name="buildSuccessLog">성공 시 남길 로그. 성공했을 때만 평가된다.</param>
        private async Task EnterRoom(
            Func<int, Task<StartGameResult>> request,
            Func<string> buildSuccessLog)
        {
            SetUIInteractable(false);
            var entered = false;

            try
            {
                GameManager.Instance.Nickname = NicknameText;

                var sceneIndex = SceneName.GetIndex(SceneName.Room);
                var result = await request(sceneIndex);

                entered = result is { Ok: true };
                if (entered)
                    Debug.Log(buildSuccessLog());
                else
                    ShowMessage(ShutdownReasonMessage.Get(result));
            }
            catch (Exception e)
            {
                // async void 핸들러에서 예외가 새면 UI가 잠긴 채로 영구히 멈춘다.
                Debug.LogException(e);
                ShowMessage(ShutdownReasonMessage.Unknown);
            }
            finally
            {
                // 성공하면 씬이 전환되므로 잠금을 유지한다. 실패·예외일 때만 되돌린다.
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
