using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace Network
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
            if (Runner != null && Runner.IsRunning)
                return Runner;
            
            Runner = Instantiate(runnerPrefab, transform);
            Runner.AddCallbacks(Events);
            return Runner;
        }

        public async Task RemoveRunner()
        {
            if (Runner == null || !Runner.IsRunning) return;
            await Runner.Shutdown();
            Destroy(Runner);
            Runner = null;
        }

        private NetworkEvents CreateEvents()
        {
            if (Events != null) return Events;
            Events = Instantiate(eventsPrefab, transform);
            return Events;
        }
    }
}
