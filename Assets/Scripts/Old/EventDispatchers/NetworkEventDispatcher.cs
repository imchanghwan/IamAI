using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace EventDispatchers
{
    public class NetworkEventDispatcher : MonoBehaviour, INetworkRunnerCallbacks
    {
        public event Action<NetworkRunner, PlayerRef> OnPlayerJoinedEvent;
        public event Action<NetworkRunner, PlayerRef> OnPlayerLeftEvent;
        public event Action<NetworkRunner, ShutdownReason> OnShutdownEvent;
        public event Action<NetworkRunner> OnConnectedToServerEvent;
        public event Action<NetworkRunner, NetDisconnectReason> OnDisconnectedEvent;
        public event Action<NetworkRunner, List<SessionInfo>> OnSessionListUpdatedEvent;
        public event Action<NetworkRunner, NetworkInput> OnInputEvent;
        public event Action<NetworkRunner> OnSceneLoadDoneEvent;

        private NetworkRunner _runner;

        /// <summary>
        /// 외부에서 NetworkRunner를 주입받아 콜백을 등록하는 초기화 메서드
        /// </summary>
        public void Init(NetworkRunner runner)
        {
            _runner = runner;
            _runner.AddCallbacks(this);
        }

        private void OnDestroy()
        {
            if (_runner != null)
            {
                _runner.RemoveCallbacks(this);
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            OnPlayerJoinedEvent?.Invoke(runner, player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            OnPlayerLeftEvent?.Invoke(runner, player);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            OnShutdownEvent?.Invoke(runner, shutdownReason);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            OnConnectedToServerEvent?.Invoke(runner);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            OnDisconnectedEvent?.Invoke(runner, reason);
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            OnSessionListUpdatedEvent?.Invoke(runner, sessionList);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            OnInputEvent?.Invoke(runner, input);
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            OnSceneLoadDoneEvent?.Invoke(runner);
        }
        
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}