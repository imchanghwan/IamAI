using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerCard : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text Nickname { get; private set; }

        public void SetNickname(string nickname)
        {
            Nickname.text = nickname;
        }
    }
}
