using System;
using IamAI.Input;
using IamAI.Network;
using UnityEngine;
using UnityEngine.UI;

namespace IamAI.Pause
{
    public class InGameMenuPopup : MonoBehaviour
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
            exitButton.interactable = false;

            try
            {
                await NetworkManager.Instance.RemoveRunner();
                // 성공하면 셧다운 → PlayerDataSpawner가 로비로 돌린다. 버튼을 되돌리지 않는다.
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
            InputManager.Instance.EnableGameplayInput();
        }
    }
}