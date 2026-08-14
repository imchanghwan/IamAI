using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerCard : MonoBehaviour
    {
        [SerializeField] public Image playerImage;
        [SerializeField] public TMP_Text nickname;

        public void SetPlayerImage(Sprite sprite)
        {
            playerImage.sprite = sprite;
        }
        
        public void SetNickname(string newName)
        {
            nickname.text = newName;
        }
    }
}
