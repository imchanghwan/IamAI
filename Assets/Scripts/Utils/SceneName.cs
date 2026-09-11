using UnityEngine.SceneManagement;

namespace Utils
{
    public static class SceneName
    {
        public const string Lobby = "LobbyScene";
        public const string Room = "RoomScene";
        public const string Game = "GameScene";

        public static int GetIndex(string sceneName)
        {
            return SceneUtility.GetBuildIndexByScenePath(sceneName);
        }

        public static Scene GetScene(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName);
        }

        public static string GetNameByScene(Scene scene)
        {
            return scene.name;
        }
    }
}
