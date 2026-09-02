using System;
using System.Collections.Generic;
using Fusion;
using Network;
using UnityEngine;

namespace Game
{
    public class InGameManager : Singleton<InGameManager>
    {
        
        [SerializeField] private NetworkObject playerPrefab;
        private readonly Dictionary<PlayerRef, NetworkObject> _players = new();
        
        private NetworkEvents _networkEvents;

        protected override void Awake()
        {
            base.Awake();
            _networkEvents = NetworkManager.Instance.Events;
        }

        private void OnEnable()
        {
            _networkEvents.OnSceneLoadDone.AddListener(OnSceneLoadDone);
            _networkEvents.OnInput.AddListener(OnInput);
            _networkEvents.PlayerLeft.AddListener(OnPlayerLeft);
        }

        private void OnDisable()
        {
            _networkEvents.OnSceneLoadDone.RemoveListener(OnSceneLoadDone);
        }

        private void OnSceneLoadDone(NetworkRunner runner)
        {
            if (runner.IsServer)
            {
                SpawnAllPlayers(runner);
            }
        }

        private void SpawnAllPlayers(NetworkRunner runner)
        {
            foreach (var player in runner.ActivePlayers)
            {
                _players[player] = 
                    runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            }
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(new NetworkInputData
            {
                MoveDirection = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                ).normalized
            });
        }

        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_players.Remove(player, out var obj))
                runner.Despawn(obj);
        }
    }
}
