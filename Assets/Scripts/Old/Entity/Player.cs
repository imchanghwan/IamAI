using Camera;
using Core;
using Fusion;
using Network;
using UnityEngine;

namespace Entity
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private float speed = 5f;
        [Networked] public NetworkString<_16> Nickname { get; private set; }
        
        private Rigidbody2D _rb;
        
        public override void Spawned()
        {
            _rb = GetComponent<Rigidbody2D>();

            if (HasInputAuthority)
            {
                UnityEngine.Camera.main?.GetComponent<CameraFollow>()?.SetTarget(transform);
                var myNickname = LocalDataManager.Instance.Nickname;
                RPC_SetNickname(myNickname);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            Nickname = nickname;
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                _rb.MovePosition(_rb.position + input.MoveDirection * speed * Runner.DeltaTime);
            }
        }
    }
}
