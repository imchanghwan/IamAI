using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace IamAI.Network
{
    public class NetworkManager : SingletonPersistent<NetworkManager>
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private NetworkEvents eventsPrefab;
        public NetworkRunner Runner { get; private set; }
        public NetworkEvents Events { get; private set; }
        public NetworkSceneManagerDefault SceneManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            SceneManager = GetComponent<NetworkSceneManagerDefault>();
            Events = CreateEvents();
        }

        public NetworkRunner CreateRunner()
        {
            if (Runner != null)
                return Runner;
            
            Runner = Instantiate(runnerPrefab, transform);
            Runner.AddCallbacks(Events);
            return Runner;
        }

        /// <summary>
        /// 현재 러너를 종료하고 참조를 비운다.
        /// NetworkRunner는 1회용이므로 실패한 러너도 반드시 이 경로로 정리해야 한다.
        /// </summary>
        public async Task RemoveRunner()
        {
            var runner = Runner;
            // await 이전에 비워서 정리 중 CreateRunner()가 죽은 러너를 반환하지 않도록 한다.
            Runner = null;

            // 이미 파괴된 러너는 Unity fake null로 걸러진다.
            if (runner == null) return;

            // StartGame 실패 등으로 이미 셧다운된 러너는 GameObject만 남아있을 수 있다.
            if (runner.IsShutdown)
            {
                Destroy(runner.gameObject);
                return;
            }

            // Shutdown(destroyGameObject: true)이 GameObject까지 파괴하므로 별도 Destroy는 중복이다.
            await runner.Shutdown();
        }

        private NetworkEvents CreateEvents()
        {
            if (Events != null) return Events;
            Events = Instantiate(eventsPrefab, transform);
            return Events;
        }
    }
}
