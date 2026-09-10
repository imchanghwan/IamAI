using System.Collections.Generic;
using Fusion;

namespace Player
{
    public class PlayerDataManager : SingletonPersistent<PlayerDataManager>
    {
        private readonly Dictionary<PlayerRef, PlayerNetworkData> _players = new();

        public void Add(PlayerRef player, PlayerNetworkData data)
        {
            _players.TryAdd(player, data);
        }

        public void Set(PlayerRef player, PlayerNetworkData newData)
        {
            if (!_players.ContainsKey(player)) return;
            
            _players[player] = newData;
        }

        public void Remove(PlayerRef player)
        {
            _players.Remove(player);
        }

        public bool Remove(PlayerRef player, out PlayerNetworkData data)
        {
            return _players.Remove(player, out data);
        }

        public bool Get(PlayerRef player, out PlayerNetworkData data)
        {
            return _players.TryGetValue(player, out data);
        }

        public bool Contains(PlayerRef player)
        {
            return _players.ContainsKey(player);
        }

        public Dictionary<PlayerRef, PlayerNetworkData>.Enumerator GetEnumerator()
        {
            return _players.GetEnumerator();
        }
    }
}