using UnityEngine;
using Utils;

public class GameManager : SingletonPersistent<GameManager>
{
    public string Nickname
    {
        get => PlayerPrefs.GetString(PrefKeys.Nickname, string.Empty);
        set => PlayerPrefs.SetString(PrefKeys.Nickname, value);
    }
}