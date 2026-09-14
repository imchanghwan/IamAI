using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using IamAI.Utils;

namespace IamAI.Network
{
    public class SessionManager : SingletonPersistent<SessionManager>
    {
        public const int MinPlayers = 2;
        public const int MaxPlayers = 8;

        /// <summary>방 코드 자릿수. 생성(CreateRoom)과 검증(IsValidRoomCode)이 이 값을 공유한다.</summary>
        public const int RoomCodeLength = 4;

        /// <summary>
        /// 방 코드가 참가에 쓸 수 있는 형식인지 검사한다.
        /// Fusion은 빈 SessionName을 "아무 공개방에나 참가"로 처리하므로,
        /// 코드 참가 전에 반드시 통과시켜야 한다.
        /// </summary>
        public static bool IsValidRoomCode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != RoomCodeLength) return false;

            foreach (var c in code)
            {
                if (c < '0' || c > '9') return false;
            }

            return true;
        }

        public int PlayerCount => RoomInfo.PlayerCount;
        
        public string RoomCode => (string)RoomInfo?.Properties[PrefKeys.RoomCode];
        public bool IsPrivate => (bool)RoomInfo?.Properties[PrefKeys.IsPrivate];
        
        private SessionInfo RoomInfo => NetworkManager.Instance?.Runner?.SessionInfo;

        private const int MaxRetries = 10;

        /// <summary>
        /// 세션의 참가 가능 여부를 바꾼다. 게임 시작 시 닫아 진행 중인 게임으로의 난입을 막는다.
        /// 세션 속성 변경은 호스트만 가능하므로 클라이언트에서 호출하면 무시된다.
        /// </summary>
        public void SetJoinable(bool joinable)
        {
            var runner = NetworkManager.Instance.Runner;
            if (runner == null || !runner.IsServer) return;

            var info = runner.SessionInfo;
            if (info == null || !info.IsValid) return;

            info.IsOpen = joinable;
        }
        
        public async Task<StartGameResult> MatchQuick(int sceneIndex)
        {
            Debug.Log("[QuickJoin] 공개방 검색 중...");

            var joinProps = new Dictionary<string, SessionProperty>
            {
                { PrefKeys.IsPrivate, false }
            };

            var joinResult = await StartGame(GameMode.Client, string.Empty, sceneIndex, customProps: joinProps);
            // var joinResult = await StartGame(GameMode.Client, string.Empty, sceneIndex);

            if (joinResult.Ok)
                return joinResult;

            Debug.Log("[QuickJoin] 참가 가능한 공개방이 없습니다. 새 공개방을 생성합니다.");
            return await CreateRoom(sceneIndex, isPrivate: false);
        }

        public async Task<StartGameResult> CreateRoom(int sceneIndex, bool isPrivate = false)
        {
            for (int i = 0; i < MaxRetries; i++)
            {
                var code   = RandomCodeGenerator.GenerateNumbers(RoomCodeLength);
                var result = await StartGame(
                    GameMode.Host, code, sceneIndex,
                    customProps: new Dictionary<string, SessionProperty>
                    {
                        { PrefKeys.RoomCode,  code      },
                        { PrefKeys.IsPrivate, isPrivate }
                    },
                    isVisible: !isPrivate);

                if (result.Ok) return result;

                if (result.ShutdownReason == ShutdownReason.GameIdAlreadyExists)
                {
                    Debug.LogWarning($"[{code}] 코드 충돌, 재시도 ({i + 1}/{MaxRetries})");
                    continue;
                }

                return result;
            }

            Debug.LogError("방 생성 최대 재시도 횟수를 초과했습니다.");
            return null;
        }

        public async Task<StartGameResult> JoinRoom(string roomCode, int sceneIndex)
        {
            return await StartGame(GameMode.Client, roomCode, sceneIndex);
        }

        private async Task<StartGameResult> StartGame(
            GameMode gameMode, string sessionName, int sceneIndex,
            int maxPlayers = MaxPlayers, Dictionary<string, SessionProperty> customProps = null,
            bool isVisible = true, bool isOpen = true)
        {
            var runner = NetworkManager.Instance.CreateRunner();
            runner.ProvideInput = true;

            var sceneManager = NetworkManager.Instance.SceneManager;

            StartGameResult result;
            try
            {
                result = await runner.StartGame(new StartGameArgs
                {
                    GameMode          = gameMode,
                    SessionName       = sessionName,
                    Scene             = SceneRef.FromIndex(sceneIndex),
                    SceneManager      = sceneManager,
                    PlayerCount       = maxPlayers,
                    SessionProperties = customProps,
                    IsVisible         = isVisible,
                    IsOpen            = isOpen
                });
            }
            catch
            {
                // 예외로 중단된 러너도 재사용할 수 없다.
                await NetworkManager.Instance.RemoveRunner();
                throw;
            }

            // NetworkRunner는 1회용. 실패한 러너를 남겨두면 다음 CreateRunner()가 그대로 반환해
            // 이미 사용된 러너로 StartGame을 다시 호출하게 된다.
            if (!result.Ok)
                await NetworkManager.Instance.RemoveRunner();

            return result;
        }
    }
}
