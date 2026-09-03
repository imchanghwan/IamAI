using Fusion;
using Game;
using UnityEngine;
using Utils;

namespace Player
{
    public struct PlayerNetworkData : INetworkStruct
    {
        [Networked] public NetworkString<_32> Nickname { get; private set; }
    }
}
