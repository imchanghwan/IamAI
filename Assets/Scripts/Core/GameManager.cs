using UnityEngine;
using Utils;

namespace Core
{
    public class GameManager : SingletonPersistent<GameManager>
    {
        public string LocalNickname
        {
            get => PlayerPrefs.GetString(PrefKeys.Nickname);
            set => PlayerPrefs.SetString(PrefKeys.Nickname, value);
        }
    }
}
