using Fusion;
using Game;
using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerNetworkData : NetworkBehaviour
    {
        public NetworkString<_32> Nickname { get; private set; }
    
        public override void Spawned()
        {
            if (!HasInputAuthority) return;
            
            string savedNickname = GameManager.Instance.Nickname;
            RPC_SetNickname(savedNickname);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            Nickname = nickname;
        }
    }
}
