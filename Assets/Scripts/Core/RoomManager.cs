using Fusion;
using Network;
using UnityEngine;
using Utils;

namespace Core
{
    public class RoomManager : SingletonPersistent<RoomManager>
    {
        [field: SerializeField] public RectTransform PlayerUIContainer { get; private set; }
        
        public SessionInfo RoomInfo
        {
            get
            {
                if (NetworkManager.Instance.Runner == null) 
                {
                    Debug.LogWarning("Runner가 생성되지 않았음");
                    return null;
                }
                
                if (NetworkManager.Instance.Runner.SessionInfo == null) 
                {
                    Debug.LogWarning("Session이 생성되지 않았음");
                    return null;
                }
                
                return NetworkManager.Instance.Runner.SessionInfo;
            }
        }

        public string RoomCode => (string)RoomInfo.Properties[PrefKeys.RoomCode];
        public bool IsPrivate => (int)RoomInfo.Properties[PrefKeys.IsPrivate] == 1;
    }
}
