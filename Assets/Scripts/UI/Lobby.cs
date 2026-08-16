using System;
using Core;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class Lobby : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInput;
        [SerializeField] private TMP_InputField roomCodeInput;
    
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;

        private string Nickname => nicknameInput.text;
        private string RoomCode => roomCodeInput.text;

        private void Start()
        {
            nicknameInput.text = GameManager.Instance.LocalNickname;
        }

        private void OnEnable()
        {
            quickMatchButton.onClick.AddListener(HandleQuickMatchButton);
            createButton.onClick.AddListener(HandleCreateButton);
            joinButton.onClick.AddListener(HandleJoinButton);
        }

        private void OnDisable()
        {
            quickMatchButton.onClick.RemoveListener(HandleQuickMatchButton);
            createButton.onClick.RemoveListener(HandleCreateButton);
            joinButton.onClick.RemoveListener(HandleJoinButton);
        }

        private async void HandleQuickMatchButton()
        {
            SetNickname(Nickname);
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            await NetworkManager.Instance.Connection.MatchQuick(sceneIndex);
        }

        private async void HandleCreateButton()
        {
            SetNickname(Nickname);
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            await NetworkManager.Instance.Connection.CreateRoom(sceneIndex);
        }
    
        private async void HandleJoinButton()
        {
            SetNickname(Nickname);
            int sceneIndex = SceneName.GetIndex(SceneName.Room);
            await NetworkManager.Instance.Connection.JoinRoom(RoomCode, sceneIndex);
        }

        private void SetNickname(string nickname)
        {
            GameManager.Instance.LocalNickname = nickname;
        }
    }
}