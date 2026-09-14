using UnityEngine;

namespace IamAI
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // 지연 탐색(FindAnyObjectByType)을 두지 않는다(P2-2).
        // 탐색은 "매니저가 씬 어딘가엔 있겠지"를 전제하는데, 씬에서 바로 시작하는
        // 테스트에선 그 전제가 깨져 조용히 null이 된다. 대신 Bootstrap이 시작 시
        // 매니저를 반드시 생성하도록 보장하고, 여기서는 캐시된 인스턴스만 돌려준다.
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
        }

        protected virtual void OnApplicationQuit() => Instance = null;

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public abstract class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
                DontDestroyOnLoad(gameObject);
        }
    }
}
