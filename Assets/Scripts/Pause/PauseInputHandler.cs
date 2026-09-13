using System;
using IamAI.Input;
using IamAI.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IamAI.Pause
{
    public class PauseInputHandler : MonoBehaviour, InputActions.IUIActions
    {
        public event Action OnPauseRequested;
        private void OnEnable()
        {
            InputManager.Instance?.Actions?.UI.SetCallbacks(this);
            InputManager.Instance?.Actions?.UI.Enable();
        }

        private void OnDisable()
        {
            InputManager.Instance?.Actions?.UI.RemoveCallbacks(this);
            InputManager.Instance?.Actions?.UI.Disable();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            OnPauseRequested?.Invoke();
        }
    }
}