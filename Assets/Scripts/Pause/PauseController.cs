using Input;
using UI;
using UnityEngine;

namespace Pause
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private InGamePausedPopup inGamePausedPopup;
        [SerializeField] private PauseInputHandler pauseInputHandler;

        private void OnEnable()
        {
            pauseInputHandler.OnPauseRequested += OnPauseRequested;
        }

        private void OnDisable()
        {
            pauseInputHandler.OnPauseRequested -= OnPauseRequested;
        }

        private void OnPauseRequested()
        {
            inGamePausedPopup.Toggle();
            if (inGamePausedPopup.IsActive) InputManager.Instance.Actions.Player.Disable();
            else InputManager.Instance.Actions.Player.Enable();
        }
    }
}