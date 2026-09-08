using System;
using Fusion;
using Game;
using Network;
using Room;
using UnityEngine;
using Utils;

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
            gameObject.name = $"Player_{player.AsIndex:0000}_{Nickname.Value}";
            
            PlayerDataManager.Instance.SetPlayer(player, this);
            
            if (!HasInputAuthority) return;
            
            string savedNickname = GameManager.Instance.Nickname;
            RPC_SetNickname(savedNickname);
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            Nickname = nickname;
        }
        
        private void OnDataChanged()
        {
            var player = Object.InputAuthority;
            gameObject.name = $"Player_{player.AsIndex:0000}_{Nickname.Value}";
            
            PlayerDataManager.Instance.SetPlayer(player, this);
            RoomManager.Instance.UpdateSlotUI(player, Nickname.Value);
        }
    }
}
