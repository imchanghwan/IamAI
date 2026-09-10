using System.Collections.Generic;
using Fusion;
using UI;
using UnityEngine;

namespace Room
{
    public class PlayerSlotUIManager : Singleton<PlayerSlotUIManager>
    {
        [SerializeField] private PlayerSlot slotPrefab;
        [SerializeField] private Transform slotContainer;
    
        private readonly Dictionary<PlayerRef, PlayerSlot> _slots = new();
        
        public void Add(PlayerRef player, string nickname)
        {
            if (_slots.ContainsKey(player))
                return;
            var slot = Instantiate(slotPrefab, slotContainer);
            _slots.Add(player, slot);
            slot.SetNicknameText(nickname);
        }

        public void Remove(PlayerRef player)
        {
            if (!_slots.Remove(player, out var slot)) return;
            Destroy(slot.gameObject);
        }

        public void Set(PlayerRef player, string nickname)
        {
            if (!_slots.TryGetValue(player, out var slot)) return;
            slot.SetNicknameText(nickname);
        }
    }
}