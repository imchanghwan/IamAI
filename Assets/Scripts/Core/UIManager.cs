using UnityEngine;

namespace Core
{
    public class UIManager : Singleton<UIManager>
    {
        [field: SerializeField] public RectTransform PlayerUIContainer { get; private set; }
    }
}
