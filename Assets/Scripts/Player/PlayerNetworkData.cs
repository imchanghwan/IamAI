using Fusion;
using Network;
using Room;

namespace Player
{
    public class PlayerNetworkData : NetworkBehaviour
    {
        [Networked, OnChangedRender(nameof(OnDataChanged))]
        public NetworkString<_32> Nickname { get; private set; }
    
        public override void Spawned()
        {
            transform.SetParent(SessionManager.Instance.transform);
            
            var player = Object.InputAuthority;
            gameObject.name = $"Player_{player.AsIndex:0000}";
            PlayerDataManager.Instance.Add(player, this);
            PlayerSlotUIManager.Instance.Add(player, Nickname.ToString());
    
            if (!HasInputAuthority) return;
            RPC_SetNickname(GameManager.Instance.Nickname);
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            var player = Object.InputAuthority;
            PlayerDataManager.Instance.Remove(player);
            PlayerSlotUIManager.Instance.Remove(player);
        }

        private void OnDataChanged()
        {
            var player = Object.InputAuthority;
            gameObject.name = $"Player_{player.AsIndex:0000}";
            PlayerDataManager.Instance.Set(player, this);
            PlayerSlotUIManager.Instance.Set(player, Nickname.ToString());
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            Nickname = nickname;
        }
    }
}
