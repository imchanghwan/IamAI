using Fusion;
using Game;
using Network;
using UnityEngine;

namespace Player
{
    public class PlayerNetworkSlot : NetworkBehaviour
    {
        [Header("Network Data")]
        [Networked, OnChangedRender(nameof(OnDataChanged))]
        public NetworkString<_32> Nickname { get; private set; }
    
        public override void Spawned()
        {
            var player = Object.InputAuthority;
            gameObject.name = $"Player_{player.AsIndex}_{Nickname.Value}";
            RoomManager.Instance.AddSlotUI(player, Nickname.Value);
            SessionManager.Instance.AddPlayer(player, new PlayerData(player, Nickname.Value));
            // 나 자신의 오브젝트라면 내 데이터 전송
            if (!HasInputAuthority) return;
            
            string savedNickname = GameManager.Instance.Nickname;
            RPC_SetNickname(savedNickname);
        }
    
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            RoomManager.Instance.RemoveSlotUI(Object.InputAuthority);
        }

        // 클라이언트 → Host로 데이터 전송
        // Host가 [Networked] 속성을 변경 → 전체 복제
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            Nickname = nickname;
        }

        // [Networked] 값이 바뀌면 모든 클라이언트에서 실행
        private void OnDataChanged()
        {
            var player = Object.InputAuthority;
            gameObject.name = $"Player_{player.AsIndex}_{Nickname.Value}";
            RoomManager.Instance.UpdateSlotUI(player, Nickname.Value);
            SessionManager.Instance.SetPlayer(player, new PlayerData(player, Nickname.Value));
        }
    }
}