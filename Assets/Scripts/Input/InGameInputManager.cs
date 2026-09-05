using Fusion;
using Network;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InGameInputManager : Singleton<InGameInputManager>, InputActions.IPlayerActions
    {
        private NetworkEvents _networkEvents;
        private InputActions _actions;
        
        private NetworkInputData _inputData;

        private void OnEnable()
        {
            _networkEvents = NetworkManager.Instance.Events;
            _actions ??= new InputActions();

            _networkEvents.OnInput.AddListener(OnInput);

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
            _inputData = new NetworkInputData
            {
                MoveDirection = context.ReadValue<Vector2>()
            };
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(_inputData);
        }
    }
}