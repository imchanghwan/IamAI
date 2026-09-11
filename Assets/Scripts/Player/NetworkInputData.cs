using Fusion;
using UnityEngine;

namespace Player
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 MoveDirection;
        public NetworkBool IsDashing;
    }
}