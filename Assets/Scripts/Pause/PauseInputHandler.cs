using System;
using Input;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pause
{
    public class PauseInputHandler : MonoBehaviour, InputActions.IUIActions
    {
        public event Action OnPauseRequested;
        private void OnEnable()
        {
            InputManager.Instance.Actions?.UI.SetCallbacks(this);
            InputManager.Instance.Actions?.UI.Enable();
        }

        private void OnDisable()
        {
            InputManager.Instance.Actions?.UI.RemoveCallbacks(this);
            InputManager.Instance.Actions?.UI.Disable();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            OnPauseRequested?.Invoke();
        }
    }
}