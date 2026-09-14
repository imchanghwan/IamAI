using System;
using System.Collections.Generic;
using Fusion;
using IamAI.Input;
using IamAI.Network;
using IamAI.Player;
using UnityEngine;

namespace IamAI.Game
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

            // Spawn 이전에 확인한다. Spawn 후 TryAdd 순서면 같은 플레이어에 두 번 호출될 때
            // 두 번째 아바타가 등록부에 못 들어간 채 남아 추적도 정리도 불가능해진다.
            // (OnSceneLoadDone의 SpawnAllPlayers와 OnPlayerJoined가 겹칠 수 있다)
            if (_players.ContainsKey(player)) return;

            var obj = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            _players.Add(player, obj);

            // SetPlayerObject는 호출하지 않는다. PlayerObject는 데이터 객체 전용이고,
            // 여기서 덮어쓰면 데이터 객체를 잃어버려 정리되지 않는다(P0-4).
            // 아바타는 위 _players 등록부로 추적한다.
        }
        
    }
}
