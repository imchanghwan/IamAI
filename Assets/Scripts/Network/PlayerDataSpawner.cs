using Fusion;
using IamAI.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using IamAI.Utils;

namespace IamAI.Network
{
    /// <summary>
    /// 플레이어 데이터 객체의 생성·정리를 담당한다.
    /// 씬에 종속되면 GameScene에서 나간 플레이어의 데이터 객체를 정리할 주체가 사라지므로,
    /// 씬 독립(DDOL)으로 유지한다(P0-4 · P1-1).
    /// </summary>
    public class PlayerDataSpawner : SingletonPersistent<PlayerDataSpawner>
    {
        [Header("Network")]
        [SerializeField] private NetworkObject playerNetworkData;

        private NetworkEvents _networkEvents;

        protected override void Awake()
        {
            base.Awake();
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
            if (_networkEvents == null) return;

            _networkEvents.PlayerJoined.RemoveListener(OnPlayerJoined);
            _networkEvents.PlayerLeft.RemoveListener(OnPlayerLeft);
            _networkEvents.OnShutdown.RemoveListener(OnShutDown);
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;
            if (PlayerDataManager.Instance.Contains(player)) return;

            var obj = runner.Spawn(playerNetworkData, inputAuthority: player);

            // PlayerObject는 데이터 객체 전용이다. 아바타로 덮어쓰지 않는다(P0-4).
            runner.SetPlayerObject(player, obj);
        }

        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;

            // PlayerObject 대신 등록부에서 찾는다. 어느 씬에 있든 같은 경로로 정리된다.
            if (!PlayerDataManager.Instance.Get(player, out var dataObject)) return;
            if (dataObject == null) return;

            // Despawned()가 등록부에서 빼고, 등록부 이벤트가 슬롯 UI를 정리한다.
            runner.Despawn(dataObject.Object);
        }

        private void OnShutDown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            // 사유를 아는 건 여기뿐이고 보여줄 곳은 Lobby라, 씬을 넘겨 전달한다.
            // Ok는 사용자가 직접 나가기를 눌러 정상 종료된 경우라 안내가 필요 없다.
            if (shutdownReason != ShutdownReason.Ok)
            {
                // 매핑이 Unknown으로 떨어질 때 실제 사유를 추적할 수 있도록 원본 값도 남긴다.
                Debug.Log($"[Shutdown] {shutdownReason}");
                GameManager.Instance.SetPendingMessage(ShutdownReasonMessage.Get(shutdownReason));
            }

            SceneManager.LoadScene(SceneName.Lobby);
        }
    }
}
