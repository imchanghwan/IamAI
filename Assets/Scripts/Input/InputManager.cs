namespace IamAI.Input
{
    public class InputManager : SingletonPersistent<InputManager>
    {
        public InputActions Actions { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            // 중복 인스턴스는 base.Awake에서 파괴 예정이다. 그 경우 InputActions를
            // 만들지 않는다(만들면 OnDestroy에서 곧바로 Dispose하는 헛일이 된다).
            if (Instance != this) return;

            Actions = new InputActions();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // InputActions는 내부에 네이티브 입력 에셋을 잡는다. C# 객체가 사라져도
            // 자동으로 풀리지 않으므로 명시적으로 Dispose()해야 누수가 없다(P2-2).
            // 중복 인스턴스는 base.Awake에서 즉시 파괴되며, 그 경우 Actions는 아직 null이다.
            Actions?.Dispose();
        }

        // 입력맵 켜고 끄기는 전부 여기를 거친다(P2-4). 호출부는 "무슨 맵을 켠다/끈다"만
        // 알면 되고, Actions.Player/UI를 직접 만지지 않는다.
        // Player(이동)와 UI(일시정지)는 상호배타가 아니라 인게임 중 함께 켜질 수 있으므로
        // 하나의 컨텍스트로 묶지 않고 맵별로 토글한다.
        public void EnableGameplayInput() => Actions.Player.Enable();
        public void DisableGameplayInput() => Actions.Player.Disable();

        public void EnableMenuInput() => Actions.UI.Enable();
        public void DisableMenuInput() => Actions.UI.Disable();
    }
}