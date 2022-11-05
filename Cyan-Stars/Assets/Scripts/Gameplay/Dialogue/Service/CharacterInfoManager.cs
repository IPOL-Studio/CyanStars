using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    public class CharacterInfoManager : MonoBehaviour, IService
    {
        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private Image avatar;

        private void Start()
        {
            GameRoot.Dialogue.RegisterOrReplaceService(this);
        }

        public void OnRegister()
        {
            nameText.text = string.Empty;
        }

        public void OnUnregister()
        {
        }

        public void SetNameText(string text)
        {
            nameText.text = text;
        }

        public async Task SetAvatar(string filePath)
        {
            avatar.sprite = (await GameRoot.Asset.AwaitLoadAsset<Sprite>(filePath, gameObject));
        }
    }
}
