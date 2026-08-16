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
    }
}
