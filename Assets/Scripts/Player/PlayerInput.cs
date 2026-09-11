using Fusion;
using Input;
using Network;
using UI;

namespace Player
{
    public class PlayerInput : NetworkBehaviour
    {
        private NetworkEvents NetworkEvents => NetworkManager.Instance.Events;
        

        public override void Spawned()
        {
            if (!HasInputAuthority) return;

            NetworkEvents?.OnInput.AddListener(OnInput);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority) return;

            NetworkEvents?.OnInput.RemoveListener(OnInput);
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(InGameInputManager.Instance.InputData);
        }
    }
}