using System;
using System.Collections.Generic;
using Fusion;

namespace IamAI.Player
{
    /// <summary>
    /// 플레이어 데이터 객체의 등록부. 씬과 무관하게 유지된다.
    /// UI는 이 클래스를 직접 호출하지 않고 이벤트를 구독한다(P1-1).
    /// </summary>
    public class PlayerDataManager : SingletonPersistent<PlayerDataManager>
    {
        private readonly Dictionary<PlayerRef, PlayerDataObject> _players = new();

        /// <summary>플레이어가 등록됐다.</summary>
        public event Action<PlayerRef, PlayerDataObject> Added;

        /// <summary>등록된 플레이어의 데이터가 바뀌었다(닉네임 등).</summary>
        public event Action<PlayerRef, PlayerDataObject> Changed;

        /// <summary>플레이어가 등록부에서 빠졌다.</summary>
        public event Action<PlayerRef> Removed;

        /// <summary>현재 등록된 플레이어 수.</summary>
        public int Count => _players.Count;

        public void Add(PlayerRef player, PlayerDataObject dataObject)
        {
            if (!_players.TryAdd(player, dataObject)) return;
            Added?.Invoke(player, dataObject);
        }

        public void Set(PlayerRef player, PlayerDataObject newDataObject)
        {
            if (!_players.ContainsKey(player)) return;

            _players[player] = newDataObject;
            Changed?.Invoke(player, newDataObject);
        }

        public void Remove(PlayerRef player)
        {
            if (!_players.Remove(player)) return;
            Removed?.Invoke(player);
        }

        public bool Remove(PlayerRef player, out PlayerDataObject dataObject)
        {
            if (!_players.Remove(player, out dataObject)) return false;

            Removed?.Invoke(player);
            return true;
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
