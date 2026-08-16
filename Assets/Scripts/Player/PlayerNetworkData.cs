using Core;
using Fusion;

namespace Player
{
    public class PlayerNetworkData : NetworkBehaviour
    {
        [Networked] public NetworkString<_16> Nickname { get; private set; }
        [Networked] public PlayerRef Player { get; set; }
        public override void Spawned()
        {
            if (HasInputAuthority)
            {
                var myNickname = GameManager.Instance.LocalNickname;
                RPC_SetNickname(myNickname);
            }
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            Nickname = nickname;
        }
    }
}
