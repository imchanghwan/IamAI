using Fusion;
using Input;
using Network;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInput : NetworkBehaviour, InputActions.IPlayerActions
    {
        private NetworkInputData _inputData;
        private NetworkEvents NetworkEvents => NetworkManager.Instance.Events;
        
        public override void Spawned()
        {
            if (!HasInputAuthority) return;

            InputManager.Instance.Actions.Player.SetCallbacks(this);
            InputManager.Instance.Actions.Player.Enable();
            
            NetworkEvents?.OnInput.AddListener(OnInput);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority) return;

            InputManager.Instance.Actions.Player.RemoveCallbacks(this);
            InputManager.Instance.Actions.Player.Disable();
            
            NetworkEvents?.OnInput.RemoveListener(OnInput);
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(_inputData);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _inputData = new NetworkInputData
            {
                MoveDirection = context.ReadValue<Vector2>()
            };
        }
    }
}