using Fusion;
using IamAI.Input;
using IamAI.Network;
using IamAI.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IamAI.Player
{
    public class PlayerInput : NetworkBehaviour, InputActions.IPlayerActions
    {
        private NetworkInputData _inputData;
        private NetworkEvents NetworkEvents => NetworkManager.Instance.Events;
        
        public override void Spawned()
        {
            if (!HasInputAuthority) return;

            // 바인딩은 이 컴포넌트에 묶여 로컬 유지, 맵 on/off만 InputManager 경유(P2-4).
            InputManager.Instance.Actions.Player.SetCallbacks(this);
            InputManager.Instance.EnableGameplayInput();
            
            NetworkEvents?.OnInput.AddListener(OnInput);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority) return;

            InputManager.Instance.Actions.Player.RemoveCallbacks(this);
            InputManager.Instance.DisableGameplayInput();
            
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