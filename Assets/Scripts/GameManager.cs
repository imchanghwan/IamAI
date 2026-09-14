namespace IamAI
{
    public class GameManager : SingletonPersistent<GameManager>
    {
        public string Nickname { get; set; }
        // {
        //     get => PlayerPrefs.GetString(PrefKeys.Nickname, string.Empty);
        //     set => PlayerPrefs.SetString(PrefKeys.Nickname, value);
        // }

        /// <summary>
        /// 로비에서 보여줄 안내 문구를 씬 너머로 전달한다.
        /// 세션 종료 사유는 Room·Game 씬에서만 알 수 있고 표시는 Lobby에서 해야 하므로,
        /// 씬 독립 객체가 잠시 들고 있어야 한다.
        /// </summary>
        private string _pendingMessage;

        public void SetPendingMessage(string message)
        {
            _pendingMessage = message;
        }

        /// <summary>
        /// 대기 중인 문구를 꺼내고 비운다. 남겨두면 다음에 로비로 돌아올 때 또 뜬다.
        /// </summary>
        public bool TryTakePendingMessage(out string message)
        {
            message = _pendingMessage;
            _pendingMessage = null;
            return !string.IsNullOrEmpty(message);
        }
    }
}
