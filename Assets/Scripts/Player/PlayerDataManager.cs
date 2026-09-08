using System.Collections.Generic;
using Fusion;

namespace Player
{
    public class PlayerDataManager : SingletonPersistent<PlayerDataManager>
    {
        private readonly Dictionary<PlayerRef, PlayerNetworkData> _players = new();

        public void AddPlayer(PlayerRef player, PlayerNetworkData data)
        {
            _players.TryAdd(player, data);
        }

        public void SetPlayer(PlayerRef player, PlayerNetworkData data)
        {
            _players[player] = data;
        }

        public void RemovePlayer(PlayerRef player)
        {
            _players.Remove(player);
        }

        public bool RemovePlayer(PlayerRef player, out PlayerNetworkData data)
        {
            return _players.Remove(player, out data);
        }

        public bool GetPlayer(PlayerRef player, out PlayerNetworkData data)
        {
            return _players.TryGetValue(player, out data);
        }

        public bool ContainsPlayer(PlayerRef player)
        {
            return _players.ContainsKey(player);
        }

        public Dictionary<PlayerRef, PlayerNetworkData>.Enumerator GetEnumerator()
        {
            return _players.GetEnumerator();
        }
    }
}