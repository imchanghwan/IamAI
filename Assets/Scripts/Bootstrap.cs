using UnityEngine;

namespace IamAI
{
    /// <summary>
    /// 어느 씬에서 시작하든 매니저들이 존재하도록 보장한다(P2-2).
    ///
    /// 지금은 매니저가 LobbyScene·RoomScene에 흩어져 있어, 에디터에서 GameScene 등으로
    /// 바로 시작하면 매니저가 없어 NullReference가 난다. 이 부트스트랩은 첫 씬이 로드되기
    /// 전에 <c>Resources/Managers</c> 프리팹을 한 번 생성해, 시작 씬과 무관하게 매니저를
    /// 세운다. PC 테스트에서 임의의 씬으로 진입할 수 있게 하는 것이 목적이다.
    ///
    /// 프리팹이 없으면 아무 일도 하지 않으므로, 매니저를 씬에 직접 둔 기존 방식과도 호환된다.
    /// (프리팹을 두고 씬에도 남겨두면 중복이 생기지만 <see cref="SingletonPersistent{T}"/>가
    /// 나중 것을 스스로 파괴한다. 정석은 프리팹으로 옮기고 씬에서는 제거하는 것이다.)
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>Resources 아래 매니저 프리팹 경로(확장자 없음).</summary>
        private const string ManagersResourcePath = "Managers";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureManagers()
        {
            var prefab = Resources.Load<GameObject>(ManagersResourcePath);
            if (prefab == null) return; // 프리팹 미구성 — 씬 배치 방식으로 동작

            // 생성된 매니저들은 각자의 Awake에서 DontDestroyOnLoad로 살아남는다.
            Object.Instantiate(prefab);
        }
    }
}
