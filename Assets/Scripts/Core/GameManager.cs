using Fusion;
using Network;
using UnityEngine;
using Utils;

namespace Core
{
    public class GameManager : SingletonPersistent<GameManager>
    {
        public string LocalNickname
        {
            get
            {
                if (PlayerPrefs.HasKey(PrefKeys.Nickname))
                {
                    return PlayerPrefs.GetString(PrefKeys.Nickname);
                }
                
                return string.Empty;
            }
            set => PlayerPrefs.SetString(PrefKeys.Nickname, value);
        }

        public SessionInfo RoomInfo
        {
            get
            {
                if (NetworkManager.Instance.Runner == null) 
                {
                    Debug.LogWarning("Runner가 생성되지 않았음");
                    return null;
                }
                
                return NetworkManager.Instance.Runner.SessionInfo;
            }
        }
    }
}
