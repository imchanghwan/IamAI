using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class RoomManager : Singleton<RoomManager>
    {
        [SerializeField] private PlayerSlot slotPrefab;
        [SerializeField] private Transform slotContainer;
    
        private readonly Dictionary<PlayerNetworkData, PlayerSlot> _slots = new();

        public void AddSlotUI(PlayerNetworkData data)
        {
            var slot =  Instantiate(slotPrefab, slotContainer);
            _slots.Add(data, slot);
            slot.UpdateUI(data);
        }

        public void RemoveSlotUI(PlayerNetworkData data)
        {
            if (!_slots.Remove(data, out var slot)) return;
            Destroy(slot.gameObject);
        }

        public void UpdateSlotUI(PlayerNetworkData data)
        {
            if (!_slots.TryGetValue(data, out var slot)) return;
            slot.UpdateUI(data);
        }
    }
}