using System;

namespace Input
{
    public class InputManager : SingletonPersistent<InputManager>
    {
        public InputActions Actions { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Actions = new InputActions();
        }

        public void SetGameContext()
        {
            Actions.UI.Disable();
            Actions.Player.Enable();
        }

        public void SetUIContext()
        {
            Actions.Player.Disable();
            Actions.UI.Enable();
        }
    }
}