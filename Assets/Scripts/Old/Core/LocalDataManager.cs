using UnityEngine;
using Utils;

namespace Core
{
    public class LocalDataManager : SingletonPersistent<LocalDataManager>
    {
        public string Nickname
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
