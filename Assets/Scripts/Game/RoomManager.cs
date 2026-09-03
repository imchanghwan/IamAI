using System.Collections.Generic;
using Fusion;
using Network;
using Player;
using UI;
using UnityEngine;

namespace Game
{
    public class RoomManager : Singleton<RoomManager>
    {
        [SerializeField] private PlayerSlot slotPrefab;
        [SerializeField] private Transform slotContainer;
    
        private readonly Dictionary<PlayerRef, PlayerSlot> _slots = new();

        public void AddSlotUI(PlayerRef player, string nickname)
        {
            var slot =  Instantiate(slotPrefab, slotContainer);
            _slots.Add(player, slot);
            slot.SetNicknameText(nickname);
        }

        public void RemoveSlotUI(PlayerRef player)
        {
            if (!_slots.Remove(player, out var slot)) return;
            Destroy(slot.gameObject);
        }

        public void UpdateSlotUI(PlayerRef player, string nickname)
        {
            if (!_slots.TryGetValue(player, out var slot)) return;
            slot.SetNicknameText(nickname);
        }
    }
}