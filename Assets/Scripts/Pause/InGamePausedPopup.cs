using Fusion;
using IamAI.Input;
using IamAI.Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using IamAI.Utils;

namespace IamAI.Pause
{
    public class InGamePausedPopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupObject;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button continueButton;
        
        public bool IsActive => popupObject.activeSelf;
        
        private void Awake()
        {
            popupObject.SetActive(false);
        }

        private void OnEnable()
        {
            exitButton.onClick.AddListener(OnClickExitButton);
            continueButton.onClick.AddListener(OnClickContinueButton);
            NetworkManager.Instance.Events.OnShutdown.AddListener(OnShutdown);
        }

        private void OnDisable()
        {
            exitButton.onClick.RemoveListener(OnClickExitButton);
            continueButton.onClick.RemoveListener(OnClickContinueButton);
        }

        public void Toggle()
        {
            popupObject.SetActive(!IsActive);
        }

        private async void OnClickExitButton()
        {
            await NetworkManager.Instance.RemoveRunner();
        }

        private void OnClickContinueButton()
        {
            popupObject.SetActive(false);
            InputManager.Instance.Actions.Player.Enable();
        }

        private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            SceneManager.LoadScene(SceneName.Lobby);
        }
    }
}