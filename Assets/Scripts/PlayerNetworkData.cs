using Fusion;
using UnityEngine;
using Utils;

public class PlayerNetworkData : NetworkBehaviour
{
    
    [Header("Network Data")]
    [Networked, OnChangedRender(nameof(OnDataChanged))]
    public NetworkString<_32> Nickname { get; private set; }
    
    public override void Spawned()
    {
        RoomManager.Instance.AddSlotUI(this);
        // 나 자신의 오브젝트라면 내 데이터 전송
        if (HasInputAuthority)
        {
            string savedNickname = PlayerPrefs.GetString(PrefKeys.Nickname, "Player");
            RPC_SetNickname(savedNickname);
        }
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        RoomManager.Instance.RemoveSlotUI(this);
    }

    // 클라이언트 → Host로 데이터 전송
    // Host가 [Networked] 속성을 변경 → 전체 복제
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetNickname(string nickname)
    {
        Nickname = nickname;
    }

    // [Networked] 값이 바뀌면 모든 클라이언트에서 실행
    private void OnDataChanged()
    {
        RoomManager.Instance.UpdateSlotUI(this);
    }
}
