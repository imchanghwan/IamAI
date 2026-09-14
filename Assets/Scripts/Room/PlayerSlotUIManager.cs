using System.Collections.Generic;
using Fusion;
using IamAI.Player;
using IamAI.UI;
using UnityEngine;

namespace IamAI.Room
{
    /// <summary>
    /// 방 화면의 플레이어 슬롯 UI. 등록부(PlayerDataManager) 이벤트를 구독한다(P1-1).
    /// 데이터 객체가 이 클래스를 직접 호출하지 않으므로, 슬롯 UI가 없는 씬에서도 문제가 없다.
    /// </summary>
    public class PlayerSlotUIManager : Singleton<PlayerSlotUIManager>
    {
        [SerializeField] private PlayerSlot slotPrefab;
        [SerializeField] private Transform slotContainer;

        private readonly Dictionary<PlayerRef, PlayerSlot> _slots = new();

        private void OnEnable()
        {
            var registry = PlayerDataManager.Instance;
            registry.Added   += OnPlayerAdded;
            registry.Changed += OnPlayerChanged;
            registry.Removed += OnPlayerRemoved;

            // 이 UI가 켜지기 전에 이미 등록된 플레이어가 있다.
            // (데이터 객체는 DDOL이라 방 씬보다 먼저 스폰돼 있을 수 있다)
            RebuildSlots(registry);
        }

        private void OnDisable()
        {
            // 앱 종료·씬 정리 중에는 등록부가 이미 파괴됐을 수 있다.
            var registry = PlayerDataManager.Instance;
            if (registry == null) return;

            registry.Added   -= OnPlayerAdded;
            registry.Changed -= OnPlayerChanged;
            registry.Removed -= OnPlayerRemoved;
        }

        /// <summary>등록부 현재 상태로 슬롯을 다시 만든다.</summary>
        private void RebuildSlots(PlayerDataManager registry)
        {
            ClearSlots();

            foreach (var pair in registry)
            {
                var dataObject = pair.Value;
                if (dataObject == null) continue;

                AddSlot(pair.Key, dataObject.Nickname.ToString());
            }
        }

        private void ClearSlots()
        {
            foreach (var slot in _slots.Values)
            {
                if (slot != null) Destroy(slot.gameObject);
            }

            _slots.Clear();
        }

        private void OnPlayerAdded(PlayerRef player, PlayerDataObject dataObject)
        {
            AddSlot(player, dataObject.Nickname.ToString());
        }

        private void OnPlayerChanged(PlayerRef player, PlayerDataObject dataObject)
        {
            if (!_slots.TryGetValue(player, out var slot)) return;
            slot.SetNicknameText(dataObject.Nickname.ToString());
        }

        private void OnPlayerRemoved(PlayerRef player)
        {
            if (!_slots.Remove(player, out var slot)) return;
            if (slot != null) Destroy(slot.gameObject);
        }

        private void AddSlot(PlayerRef player, string nickname)
        {
            if (_slots.ContainsKey(player)) return;

            var slot = Instantiate(slotPrefab, slotContainer);
            slot.SetNicknameText(nickname);
            _slots.Add(player, slot);
        }
    }
}
