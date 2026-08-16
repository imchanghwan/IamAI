using UnityEngine;

namespace Core
{
    public class RoomUIManager : Singleton<RoomUIManager>
    {
        [field: SerializeField] public RectTransform PlayerUIContainer { get; private set; }
    }
}
