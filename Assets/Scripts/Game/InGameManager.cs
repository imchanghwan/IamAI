using System;
using System.Collections.Generic;
using Fusion;
using Input;
using Network;
using Player;
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
            _networkEvents.PlayerJoined.AddListener(OnPlayerJoined);
            _networkEvents.PlayerLeft.AddListener(OnPlayerLeft);
        }

        private void OnDisable()
        {
            _networkEvents.OnSceneLoadDone.RemoveListener(OnSceneLoadDone);
            _networkEvents.PlayerJoined.RemoveListener(OnPlayerJoined);
            _networkEvents.PlayerLeft.RemoveListener(OnPlayerLeft);
        }

        private void OnSceneLoadDone(NetworkRunner runner)
        {
            SpawnAllPlayers(runner);
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            SpawnPlayer(runner, player);
        }
        
        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!_players.Remove(player, out var obj)) return;
            runner.Despawn(obj);
        }

        private void SpawnAllPlayers(NetworkRunner runner)
        {
            foreach (var player in runner.ActivePlayers)
            {
                SpawnPlayer(runner, player);
            }
        }

        private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;
            var obj = 
                runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            _players.TryAdd(player, obj);
            runner.SetPlayerObject(player, obj);
        }
        
    }
}
