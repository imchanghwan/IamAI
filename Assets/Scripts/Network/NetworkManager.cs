using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace Network
{
    public class NetworkManager : SingletonPersistent<NetworkManager>
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        public NetworkRunner Runner { get; private set; }
        public NetworkSceneManagerDefault SceneManager { get; private set; }
        public NetworkEvents Events { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Events = GetComponent<NetworkEvents>();
        }

        public NetworkRunner CreateRunner()
        {
            if (Runner != null && Runner.IsRunning)
                return Runner;
            
            Runner = Instantiate(runnerPrefab);
            return Runner;
        }

        public NetworkSceneManagerDefault CreateSceneManager()
        {
            if (SceneManager != null)
                return SceneManager;
            
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            return SceneManager;
        }

        public async Task RemoveRunner()
        {
            if (Runner == null || !Runner.IsRunning) return;
            
            await Runner.Shutdown();
            Destroy(Runner);
            Runner = null;
        }

        public void RemoveSceneManager()
        {
            if (SceneManager == null) return;
            
            Destroy(SceneManager);
            SceneManager = null;
        }
    }
}
