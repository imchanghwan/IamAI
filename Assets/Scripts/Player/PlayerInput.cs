using Fusion;
using Input;
using Network;

namespace Player
{
    public class PlayerInput : NetworkBehaviour
    {
        private NetworkEvents _networkEvents;

        private NetworkInputData InputData => InGameInputManager.Instance.InputData;

        public override void Spawned()
        {
            if (!HasInputAuthority) return;

            _networkEvents = NetworkManager.Instance.Events;
            _networkEvents?.OnInput.AddListener(OnInput);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _networkEvents?.OnInput.RemoveListener(OnInput);
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(InputData);
        }
    }
}