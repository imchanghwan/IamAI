using Core;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerCard : NetworkBehaviour
    {
        [SerializeField] public Image playerImage;
        [SerializeField] public TMP_Text nicknameText;

        [Networked] public NetworkString<_16> Nickname { get; private set; }
        
        private string _nickname;

        public override void Spawned()
        {
            var parent = RoomUIManager.Instance.PlayerUIContainer;
            transform.SetParent(parent, false);

            if (HasInputAuthority)
            {
                var nickname = LocalDataManager.Instance.Nickname;
                RPC_SetNickname(nickname);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname) => Nickname = nickname;
        
        public override void Render()
        {
            if (Nickname.Value == _nickname) return;
            _nickname = Nickname.Value;
            nicknameText.text = Nickname.Value;
        }
    }
}
