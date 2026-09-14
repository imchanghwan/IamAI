using IamAI.Entity;
using Fusion;
using IamAI.Network;
using UnityEngine;

namespace IamAI.Player
{
    public class PlayerDataObject : NetworkBehaviour
    {
        [Networked]
        public EntityNetworkData EntityData { get; private set; }
        
        [Networked, OnChangedRender(nameof(OnNicknameChanged))]
        public NetworkString<_32> Nickname { get; private set; }
    
        public override void Spawned()
        {
            transform.SetParent(SessionManager.Instance.transform);
            
            var player = Object.InputAuthority;
            gameObject.name = $"Player_{player.AsIndex:0000}";

            // 등록부에만 알린다. 슬롯 UI는 등록부 이벤트를 구독한다(P1-1).
            PlayerDataManager.Instance.Add(player, this);

            if (!HasInputAuthority) return;
            RPC_SetNickname(GameManager.Instance.Nickname);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            PlayerDataManager.Instance.Remove(Object.InputAuthority);
        }

        private void OnNicknameChanged()
        {
            var player = Object.InputAuthority;
            gameObject.name = $"Player_{player.AsIndex:0000}";
            PlayerDataManager.Instance.Set(player, this);
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            Nickname = nickname;
        }
    }
}
