using System;
using System.Collections.Generic;
using EventDispatchers;
using Fusion;
using Network;
using UnityEngine;
using Utils;

namespace Core
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private NetworkObject playerPrefab;
        private readonly Dictionary<PlayerRef, NetworkObject> _players = new();
        // private NetworkRunner _runner;
        private NetworkEventDispatcher _dispatcher;

        protected override void Awake()
        {
            base.Awake();
            // _runner = NetworkManager.Instance.Runner;
            _dispatcher = GlobalEventHub.Instance.Network;
        }

        private void OnEnable()
        {
            _dispatcher.OnSceneLoadDoneEvent += OnSceneLoadDone;
            _dispatcher.OnInputEvent += HandleInput;
            _dispatcher.OnPlayerLeftEvent += HandlePlayerLeft;
        }

        private void OnDisable()
        {
            _dispatcher.OnSceneLoadDoneEvent -= OnSceneLoadDone;
            _dispatcher.OnInputEvent -= HandleInput;
            _dispatcher.OnPlayerLeftEvent -= HandlePlayerLeft;
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

        private void HandleInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(new NetworkInputData
            {
                MoveDirection = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                ).normalized
            });
        }

        private void HandlePlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_players.Remove(player, out var obj))
                runner.Despawn(obj);
        }
    }
}
