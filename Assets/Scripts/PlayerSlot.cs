using Fusion;
using TMPro;
using UnityEngine;
using Utils;

public class PlayerSlot : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nicknameText;

    public void UpdateUI(PlayerNetworkData data)
    {
        nicknameText.text = data.Nickname.Value;
    }
}
