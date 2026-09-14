using System;
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

        private NetworkEvents _networkEvents;

        private void Awake()
        {
            popupObject.SetActive(false);
            _networkEvents = NetworkManager.Instance.Events;
        }

        private void OnEnable()
        {
            exitButton.onClick.AddListener(OnClickExitButton);
            continueButton.onClick.AddListener(OnClickContinueButton);
            _networkEvents.OnShutdown.AddListener(OnShutdown);
        }

        private void OnDisable()
        {
            exitButton.onClick.RemoveListener(OnClickExitButton);
            continueButton.onClick.RemoveListener(OnClickContinueButton);

            // NetworkEvents는 DDOL이라 씬을 넘어 살아남는다. 해제하지 않으면
            // GameScene에 재진입할 때마다 리스너가 누적돼 LoadScene이 중복 호출된다.
            // 앱 종료·씬 정리 중에는 이미 파괴됐을 수 있으므로 fake null까지 잡는 != null로 확인한다.
            if (_networkEvents != null)
                _networkEvents.OnShutdown.RemoveListener(OnShutdown);
        }

        public void Toggle()
        {
            popupObject.SetActive(!IsActive);
        }

        private async void OnClickExitButton()
        {
            exitButton.interactable = false;

            try
            {
                await NetworkManager.Instance.RemoveRunner();
                // 성공하면 OnShutdown → 로비 씬 로드. 버튼을 되돌리지 않는다.
            }
            catch (Exception e)
            {
                // async void에서 예외가 새면 나가기 버튼이 잠겨 게임에서 못 빠져나온다.
                Debug.LogException(e);
                exitButton.interactable = true;
            }
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