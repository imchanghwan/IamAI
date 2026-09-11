using System.Collections.Generic;
using Fusion;

namespace Player
{
    public class PlayerDataManager : SingletonPersistent<PlayerDataManager>
    {
        private readonly Dictionary<PlayerRef, PlayerDataObject> _players = new();

        public void Add(PlayerRef player, PlayerDataObject dataObject)
        {
            _players.TryAdd(player, dataObject);
        }

        public void Set(PlayerRef player, PlayerDataObject newDataObject)
        {
            if (!_players.ContainsKey(player)) return;
            
            _players[player] = newDataObject;
        }

        public void Remove(PlayerRef player)
        {
            _players.Remove(player);
        }

        public bool Remove(PlayerRef player, out PlayerDataObject dataObject)
        {
            return _players.Remove(player, out dataObject);
        }

        public bool Get(PlayerRef player, out PlayerDataObject dataObject)
        {
            return _players.TryGetValue(player, out dataObject);
        }

        public bool Contains(PlayerRef player)
        {
            return _players.ContainsKey(player);
        }

        public Dictionary<PlayerRef, PlayerDataObject>.Enumerator GetEnumerator()
        {
            return _players.GetEnumerator();
        }
    }
}