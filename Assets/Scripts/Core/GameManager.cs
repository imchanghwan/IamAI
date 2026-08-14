namespace Core
{
    public class GameManager : SingletonPersistent<GameManager>
    {
        public string LocalNickname { get; set; }
    }
}
