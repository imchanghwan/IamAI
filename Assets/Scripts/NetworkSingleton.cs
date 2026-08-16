using Fusion;
using UnityEngine;

public abstract class NetworkStaticInstance<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;
            
            _instance = FindAnyObjectByType<T>();
            if (_instance != null) return _instance;
            
            var objects = Resources.FindObjectsOfTypeAll<T>();
            foreach (var obj in objects)
            {
                if (obj.gameObject.scene.name != "DontDestroyOnLoad") continue;
                
                _instance = obj;
                break;
            }
            return _instance;
        }
        private set => _instance = value;
    }

    protected virtual void Awake() => Instance = this as T;

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

public abstract class NetworkSingleton<T> : NetworkStaticInstance<T> where T : NetworkBehaviour
{
    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
    }
}

public abstract class NetworkSingletonPersistent<T> : NetworkSingleton<T> where T : NetworkBehaviour
{
    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
}