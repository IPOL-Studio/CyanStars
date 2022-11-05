using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    public class BackgroundManager : MonoBehaviour, IService
    {
        [SerializeField]
        private Image background;

        private void Start()
        {
            GameRoot.Dialogue.RegisterOrReplaceService(this);
        }

        public void OnRegister()
        {
        }

        public void OnUnregister()
        {
        }

        public async Task SetBackground(string filePath)
        {
            var sprite = string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath)
                ? null
                : await GameRoot.Asset.AwaitLoadAsset<Sprite>(filePath, this.gameObject);
            background.sprite = sprite;
        }
    }
}
