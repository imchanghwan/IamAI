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
            _networkEvents.PlayerLeft.AddListener(OnPlayerLeft);
        }

        private void OnDisable()
        {
            _networkEvents.OnSceneLoadDone.RemoveListener(OnSceneLoadDone);
            _networkEvents.PlayerLeft.RemoveListener(OnPlayerLeft);
        }

        private void OnSceneLoadDone(NetworkRunner runner)
        {
            SpawnAllPlayers(runner);
        }

        private void OnPlayerJoin(NetworkRunner runner, PlayerRef player)
        {
            SpawnPlayer(runner, player);
            // SessionManager.Instance.AddPlayer(player, new PlayerData(player, ));
        }
        
        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!_players.Remove(player, out var obj)) return;
            SessionManager.Instance.RemovePlayer(player);
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
