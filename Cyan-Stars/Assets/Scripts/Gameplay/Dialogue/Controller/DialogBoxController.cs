using System;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Event;
using CyanStars.Framework.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogBoxController : MonoBehaviour
    {
        private DialogueModule dataModule;

        [SerializeField]
        private Image avatar;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI contentText;

        private void Awake()
        {
            dataModule = GameRoot.GetDataModule<DialogueModule>();
            nameText.text = string.Empty;
            contentText.text = string.Empty;

            GameRoot.Event.AddListener(EventConst.SetNameTextEvent, OnSetNameText);
            GameRoot.Event.AddListener(EventConst.SetAvatarEvent, OnSetAvatar);
            GameRoot.Timer.AddUpdateTimer(Onupdate);
        }

        private void OnDestroy()
        {
            GameRoot.Event.RemoveListener(EventConst.SetNameTextEvent, OnSetNameText);
            GameRoot.Event.RemoveListener(EventConst.SetAvatarEvent, OnSetAvatar);
            GameRoot.Timer.RemoveUpdateTimer(Onupdate);
        }

        private async void OnSetAvatar(object sender, EventArgs e)
        {
            var filePath = (e as SingleEventArgs<string>)?.Value;
            avatar.sprite = (await GameRoot.Asset.AwaitLoadAsset<Texture2D>(filePath, gameObject)).ConvertToSprite();
        }

        private void OnSetNameText(object sender, EventArgs e)
        {
            nameText.text = (e as SingleEventArgs<string>)?.Value;
        }

        private void Onupdate(float deltaTime)
        {
            if (dataModule.IsContentDirty)
            {
                contentText.text = dataModule.Content.ToString();
                dataModule.IsContentDirty = false;
            }
        }
    }
}
