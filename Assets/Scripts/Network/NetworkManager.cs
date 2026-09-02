using System.Threading.Tasks;
using Event;
using Fusion;
using UnityEngine;

namespace Network
{
    public class NetworkManager : SingletonPersistent<NetworkManager>
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        public NetworkRunner Runner { get; private set; }
        public NetworkSceneManagerDefault SceneManager { get; private set; }
        public NetworkEvent Event { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            Event = new NetworkEvent();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Runner != null)
                Runner.RemoveCallbacks(Event);
        }

        public NetworkRunner CreateRunner()
        {
            if (Runner != null && Runner.IsRunning)
                return Runner;
            
            Runner = Instantiate(runnerPrefab);
            Runner.AddCallbacks(Event);
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
            Runner.RemoveCallbacks(Event);
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
