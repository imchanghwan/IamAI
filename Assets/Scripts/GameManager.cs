using System;

public enum GameState
{
    Lobby,
    GameStart,
    Playing,
    GameOver,
    RestartVote
}

public class GameManager : SingletonPersistent<GameManager>
{
    public string Nickname { get; set; }
    // {
    //     get => PlayerPrefs.GetString(PrefKeys.Nickname, string.Empty);
    //     set => PlayerPrefs.SetString(PrefKeys.Nickname, value);
    // }

    public GameState State { get; private set; }
    public event Action<GameState> OnPhaseChanged;
    
    public void SetPhase(GameState gamePhase)
    {
        State = gamePhase;
        OnPhaseChanged?.Invoke(gamePhase);
    }
}