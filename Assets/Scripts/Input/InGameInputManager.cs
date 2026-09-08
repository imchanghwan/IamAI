using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InGameInputManager : Singleton<InGameInputManager>, InputActions.IPlayerActions
    {
        public NetworkInputData InputData { get; private set; }
        public bool Paused { get; private set; }

        public event Action<bool> OnPauseChanged;

        private InputActions _actions;

        private void OnEnable()
        {
            _actions ??= new InputActions();

            _actions.Player.SetCallbacks(this);
            _actions.Player.Enable();
        }

        private void OnDisable()
        {
            _actions.Player.RemoveCallbacks(this);
            _actions.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            InputData = new NetworkInputData
            {
                MoveDirection = context.ReadValue<Vector2>()
            };
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            Paused = !Paused;
            OnPauseChanged?.Invoke(Paused);
        }
    }
}