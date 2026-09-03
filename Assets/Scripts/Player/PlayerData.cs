using Fusion;

namespace Player
{
    [System.Serializable]
    public struct PlayerData
    {
        public PlayerRef Id { get; private set; }
        public string Nickname { get; private set; }
        
        public PlayerData(PlayerRef id, string nickname)
        {
            Id = id;
            Nickname = nickname;
        }
    }
}