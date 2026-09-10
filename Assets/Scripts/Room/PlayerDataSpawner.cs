using Fusion;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Room
{
    public class PlayerDataSpawner : MonoBehaviour
    {
        [Header("Network")]
        [SerializeField] private NetworkObject playerNetworkData;

        private NetworkEvents _networkEvents;
        private void Awake()
        {
            _networkEvents = NetworkManager.Instance.Events;
        }
        private void OnEnable()
        {
            _networkEvents.PlayerJoined.AddListener(OnPlayerJoined);
            _networkEvents.PlayerLeft.AddListener(OnPlayerLeft);
            _networkEvents.OnShutdown.AddListener(OnShutDown);
        }

        private void OnDisable()
        {
            _networkEvents.PlayerJoined.RemoveListener(OnPlayerJoined);
            _networkEvents.PlayerLeft.RemoveListener(OnPlayerLeft);
            _networkEvents.OnShutdown.RemoveListener(OnShutDown);
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Host만 Spawn
            if (!runner.IsServer) return;
            var obj = runner.Spawn(playerNetworkData, inputAuthority: player);
            runner.SetPlayerObject(player, obj);
        }

        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return; // ← 추가: Host만 Despawn
            if (!runner.TryGetPlayerObject(player, out var obj)) return;
            runner.Despawn(obj); // Despawned()가 RemovePlayer/RemoveSlotUI 처리
        }

        private void OnShutDown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            SceneManager.LoadScene(SceneName.Lobby);
        }
    }
}