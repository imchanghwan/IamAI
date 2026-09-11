using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InGameInputManager : Singleton<InGameInputManager>, InputActions.IPlayerActions
    {
        public NetworkInputData InputData { get; private set; }

        private void OnEnable()
        {
            InputManager.Instance.Actions.Player.SetCallbacks(this);
            InputManager.Instance.Actions.Player.Enable();
        }

        private void OnDisable()
        {
            InputManager.Instance.Actions.Player.RemoveCallbacks(this);
            InputManager.Instance.Actions.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            InputData = new NetworkInputData
            {
                MoveDirection = context.ReadValue<Vector2>()
            };
        }
    }
}