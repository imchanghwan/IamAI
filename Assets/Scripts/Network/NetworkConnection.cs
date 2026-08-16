using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Fusion;
using UnityEngine;
using Utils;

namespace Network
{
    public class NetworkConnection
    {
        private const int    MaxRetries   = 5;

        // 빠른참가: 공개방 검색 → 없으면 생성
        // 빠른참가: 공개방 검색 → 없으면 생성
        public async Task MatchQuick(int sceneIndex)
        {
            Debug.Log("[QuickJoin] 공개방 검색 중...");

            // 1. SessionName을 빈 문자열로 주고, SessionProperties에 필터 조건(IsPrivate == 0)을 넣습니다.
            var joinProps = new Dictionary<string, SessionProperty> 
            { 
                { PrefKeys.IsPrivate, 0 } 
            };

            // GameMode.Client로 방제 없이 접속을 시도하면, Fusion이 joinProps 조건에 맞는 방을 찾아 랜덤 접속시킵니다.
            var joinResult = await StartGame(GameMode.Client, string.Empty, sceneIndex, customProps: joinProps);

            if (joinResult.Ok)
            {
                var runner = NetworkManager.Instance.Runner;
                // 접속된 방의 프로퍼티에서 방 코드를 가져옴
                string joinedCode = (string)runner.SessionInfo.Properties[PrefKeys.RoomCode];
                Debug.Log($"[QuickJoin] 참가 성공! 방 코드: {joinedCode}");
                return;
            }

            // 2. 조건에 맞는 공개방이 없는 경우 (참가 실패 시) 새로 방 생성
            Debug.Log("[QuickJoin] 참가 가능한 공개방이 없습니다. 새 공개방을 생성합니다.");
            await CreateRoom(sceneIndex, isPrivate: false); // 이미 만들어둔 CreateRoom 재활용
        }

        // 방 생성 (공개/비공개 선택)
        public async Task CreateRoom(int sceneIndex, bool isPrivate = false)
        {
            for (int i = 0; i < MaxRetries; i++)
            {
                var code   = RandomCodeGenerator.GenerateNumbers(4);
                var result = await StartGame(
                    GameMode.Host, code, sceneIndex,
                    customProps: new Dictionary<string, SessionProperty>
                    {
                        { PrefKeys.RoomCode,  code              },
                        { PrefKeys.IsPrivate, isPrivate ? 1 : 0 }
                    },
                    isVisible: !isPrivate);

                if (result.Ok)
                {
                    Debug.Log($"[{code}] 방 생성 성공! ({(isPrivate ? "비공개" : "공개")})");
                    return;
                }

                if (result.ShutdownReason == ShutdownReason.GameIdAlreadyExists)
                {
                    Debug.LogWarning($"[{code}] 코드 충돌, 재시도 ({i + 1}/{MaxRetries})");
                    continue;
                }

                Debug.LogError($"방 생성 실패: {result.ShutdownReason}");
                return;
            }

            Debug.LogError("방 생성 최대 재시도 횟수를 초과했습니다.");
        }

        // 방 참가 (코드 직접 입력)
        public async Task JoinRoom(string roomCode, int sceneIndex)
        {
            if (string.IsNullOrEmpty(roomCode))
            {
                Debug.LogWarning("방 코드가 비어있습니다.");
                return;
            }

            var result = await StartGame(GameMode.Client, roomCode, sceneIndex);

            if (result.Ok)
                Debug.Log($"[{roomCode}] 방 참가 성공!");
            else
                Debug.LogError($"방 참가 실패: {result.ShutdownReason}");
        }

        public async Task Shutdown()
        {
            var runner = NetworkManager.Instance.Runner;
            if (runner == null || !runner.IsRunning) return;

            await runner.Shutdown();
            Debug.Log("세션 종료 완료.");
            NetworkManager.Instance.ClearRunner();
        }

        private async Task<StartGameResult> StartGame(
            GameMode gameMode, string sessionName, int sceneIndex,
            int maxPlayers = 8, Dictionary<string, SessionProperty> customProps = null,
            bool isVisible = true, bool isOpen = true)
        {
            var runner = CreateAndInitRunner();
            return await runner.StartGame(BuildArgs(
                gameMode, sessionName, sceneIndex,
                GetOrAddSceneManager(runner),
                maxPlayers, customProps, isVisible, isOpen));
        }

        private StartGameArgs BuildArgs(
            GameMode gameMode, string sessionName, int sceneIndex,
            NetworkSceneManagerDefault sceneManager,
            int maxPlayers = 8, Dictionary<string, SessionProperty> customProps = null,
            bool isVisible = true, bool isOpen = true) => new StartGameArgs
        {
            GameMode          = gameMode,
            SessionName       = sessionName,
            Scene             = SceneRef.FromIndex(sceneIndex),
            SceneManager      = sceneManager,
            PlayerCount       = maxPlayers,
            SessionProperties = customProps,
            IsVisible         = isVisible,
            IsOpen            = isOpen
        };

        private NetworkRunner CreateAndInitRunner()
        {
            var runner = NetworkManager.Instance.CreateRunner();
            runner.ProvideInput = true;
            GlobalEventHub.Instance.Network.Init(runner);
            return runner;
        }

        private static NetworkSceneManagerDefault GetOrAddSceneManager(NetworkRunner runner)
        {
            if (!runner.gameObject.TryGetComponent(out NetworkSceneManagerDefault sm))
                sm = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            return sm;
        }
    }
}