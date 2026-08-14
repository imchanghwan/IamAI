using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Network
{
    public class NetworkConnection
    {
        public async Task MatchQuick(int sceneIndex)
        {
            var result = await StartGame(GameMode.AutoHostOrClient, null, sceneIndex);

            if (result.Ok)
                Debug.Log("[QuickJoin] 빠른 참가 성공!");
            else
                Debug.LogError($"[QuickJoin] 빠른 참가 실패: {result.ShutdownReason}");
        }
        
        public async Task CreateRoom(int sceneIndex)
        {
            int retryCount = 0;
            int maxRetries = 5;
            
            while (retryCount < maxRetries)
            {
                var roomCode = RandomCodeGenerator.GenerateNumbers(4);
                var result = await StartGame(GameMode.Host, roomCode, sceneIndex);
        
                if (result.Ok)
                {
                    Debug.Log($"[{roomCode}] 방 생성 성공! (Host)");
                    return;
                }
        
                if (result.ShutdownReason == ShutdownReason.GameIdAlreadyExists)
                {
                    retryCount++;
                    Debug.LogWarning($"[{roomCode}] 이미 존재하는 방입니다. 다시 시도합니다. (시도 {retryCount}/{maxRetries})");
                    continue;
                }

                Debug.LogError($"방 생성 실패 (치명적 오류): {result.ShutdownReason}");
                return;
            }
            
            Debug.LogError("방 생성 최대 재시도 횟수를 초과했습니다.");
        }
        
        public async Task JoinRoom(string roomCode, int sceneIndex)
        {
            if (string.IsNullOrEmpty(roomCode))
            {
                Debug.LogWarning("방 코드가 비어있습니다.");
                return;
            }
            
            var result = await StartGame(GameMode.Client, roomCode, sceneIndex);

            if (result.Ok)
            {
                Debug.Log($"[{roomCode}] 방 참가 성공!");
            }
            else
            {
                Debug.LogError($"방 참가 실패: {result.ShutdownReason}");
            }
        }
        
        private async Task<StartGameResult> StartGame(
            GameMode gameMode, 
            string sessionName, 
            int sceneIndex,
            int maxPlayers = 8,
            Dictionary<string, SessionProperty> customProps = null,
            bool isVisible = true,
            bool isOpen = true
            )
        {
            var runner = NetworkManager.Instance.CreateRunner();
            runner.ProvideInput = true;
            GlobalEventHub.Instance.Network.Init(runner);
            
            if (!runner.gameObject.TryGetComponent(out NetworkSceneManagerDefault sceneManager))
            {
                sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            }
            
            return await runner.StartGame(new StartGameArgs
            {
                GameMode     = gameMode,
                SessionName  = sessionName,
                Scene     = SceneRef.FromIndex(sceneIndex),
                SceneManager = sceneManager,
                
                PlayerCount = maxPlayers,
                SessionProperties = customProps,
                IsVisible = isVisible,
                IsOpen = isOpen
            });
        }

        public async Task Shutdown()
        {
            var runner = NetworkManager.Instance.Runner;
            
            if (runner == null) return;
            
            if (runner.IsRunning)
            {
                await runner.Shutdown();
                Debug.Log("네트워크 세션 종료 완료.");
            }
            
            NetworkManager.Instance.ClearRunner();
        }
    }
}
