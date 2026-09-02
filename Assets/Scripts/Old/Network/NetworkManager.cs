using Fusion;
using Network;
using UnityEngine;

namespace Old.Network
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
