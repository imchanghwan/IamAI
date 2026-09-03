namespace Player
{
    [System.Serializable]
    public struct PlayerData
    {
        public string Nickname { get; private set; }
        
        public PlayerData(string nickname)
        {
            Nickname = nickname;
        }
    }
}