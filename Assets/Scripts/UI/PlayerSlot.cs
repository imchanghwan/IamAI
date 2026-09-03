using Player;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerSlot : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text nicknameText;

        public void SetNicknameText(string nickname)
        {
            nicknameText.text = nickname;
        }
    }
}
