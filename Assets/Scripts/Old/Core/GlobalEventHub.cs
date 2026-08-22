using EventDispatchers;
using UnityEngine;

namespace Core
{
    public class GlobalEventHub : SingletonPersistent<GlobalEventHub>
    {
        [field: SerializeField] public NetworkEventDispatcher Network { get; private set; }
        [field: SerializeField] public GameEventDispatcher Game { get; private set; }
    }
}
