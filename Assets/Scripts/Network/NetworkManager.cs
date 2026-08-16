using Fusion;
using UnityEngine;

namespace Network
{
    public class NetworkManager : SingletonPersistent<NetworkManager>
    {
        [SerializeField] private NetworkRunner runnerPrefab;
        
        public NetworkRunner Runner { get; private set; }
        
        public NetworkConnection Connection { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
            Connection = new NetworkConnection();
        }

        public NetworkRunner CreateRunner()
        {
            if (Runner != null && Runner.IsRunning)
                return Runner;
            
            if (runnerPrefab == null)
            {
                Debug.LogError("NetworkManager에 runnerPrefab이 할당되지 않았습니다!");
                return null;
            }
            
            Runner = Instantiate(runnerPrefab);
            return Runner;
        }

        public void ClearRunner()
        {
            if (Runner == null) return;
            
            Destroy(Runner.gameObject);
            Runner = null;
        }
    }
}
