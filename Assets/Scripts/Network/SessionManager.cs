using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Fusion;
using UnityEngine;
using Utils;

namespace Network
{
    public class SessionManager : SingletonPersistent<SessionManager>
    {
        private SessionInfo RoomInfo => NetworkManager.Instance?.Runner?.SessionInfo;
        
        public string RoomCode => (string)RoomInfo?.Properties[PrefKeys.RoomCode];
        public bool IsPrivate => (bool)RoomInfo?.Properties[PrefKeys.IsPrivate];
        
        private const int MaxRetries = 10;

        public void LoadScene(int sceneIndex)
        {
            var runner = NetworkManager.Instance.Runner;
            
            if (runner == null || !runner.IsRunning)
                return;

            if (!runner.IsServer)
                return;

            runner.LoadScene(SceneRef.FromIndex(sceneIndex));
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
                var code   = RandomCodeGenerator.GenerateNumbers(4);
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
            int maxPlayers = 8, Dictionary<string, SessionProperty> customProps = null,
            bool isVisible = true, bool isOpen = true)
        {
            var runner = NetworkManager.Instance.CreateRunner();
            runner.ProvideInput = true;

            var sceneManager = NetworkManager.Instance.SceneManager;
            
            return await runner.StartGame(new StartGameArgs
            {
                GameMode          = gameMode,
                SessionName       = sessionName,
                Scene          = SceneRef.FromIndex(sceneIndex),
                SceneManager      = sceneManager,
                PlayerCount       = maxPlayers,
                SessionProperties = customProps,
                IsVisible         = isVisible,
                IsOpen            = isOpen
            });
        }
    }
}
