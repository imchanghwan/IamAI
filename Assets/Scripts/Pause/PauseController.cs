using IamAI.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace IamAI.Pause
{
    /// <summary>
    /// 인게임 메뉴(일시정지)의 단일 제어자. 예전에는 입력 감지(PauseInputHandler)와
    /// 팝업 토글(PauseController)이 나뉘어 있었으나, 감지→토글이 사실상 한 흐름이라
    /// 하나로 합쳤다(P2-4, 3클래스 → 2클래스).
    /// </summary>
    public class PauseController : MonoBehaviour, InputActions.IUIActions
    {
        // 필드명을 바꿔도 씬에 직렬화된 참조가 끊기지 않도록 옛 이름을 이관한다.
        // (GameScene을 저장하면 새 이름으로 굳고, 그 뒤엔 이 특성을 지워도 된다.)
        [SerializeField, FormerlySerializedAs("inGamePausedPopup")]
        private InGameMenuPopup inGameMenuPopup;

        private void OnEnable()
        {
            // 메뉴 입력(일시정지 버튼)은 인게임 내내 살아 있어야 한다.
            // Instance는 UnityEngine.Object라 ?.가 fake null을 못 거른다 → 명시 비교(P2-3).
            var input = InputManager.Instance;
            if (input == null) return;

            // 콜백 바인딩은 이 컴포넌트에 묶이므로 로컬에 둔다. 맵 on/off만 InputManager 경유.
            input.Actions.UI.SetCallbacks(this);
            input.EnableMenuInput();
        }

        private void OnDisable()
        {
            var input = InputManager.Instance;
            if (input == null) return;

            input.Actions.UI.RemoveCallbacks(this);
            input.DisableMenuInput();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            TogglePause();
        }

        private void TogglePause()
        {
            inGameMenuPopup.Toggle();

            var input = InputManager.Instance;
            if (input == null) return;

            // 메뉴가 열리면 이동 입력을 끄고, 닫히면 되살린다.
            if (inGameMenuPopup.IsActive) input.DisableGameplayInput();
            else input.EnableGameplayInput();
        }
    }
}
