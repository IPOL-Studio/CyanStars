using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Timer;
using TMPro;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    public class TextPrinterImpl : MonoBehaviour, ITextPrinter
    {
        [SerializeField]
        private TextMeshProUGUI contentText;

        private DialogueSettingsModule settingsModule;

        private float remainingDeltaTime;

        private float stop = -1f;
        private int curCharacterCount;

        private void Start()
        {
            contentText.text = string.Empty;
            settingsModule = GameRoot.GetDataModule<DialogueSettingsModule>();
            GameRoot.Dialogue.RegisterOrReplaceService<ITextPrinter>(this);
        }

        public void OnRegister()
        {
        }

        public void OnUnregister()
        {
        }

        public Task PrintTextAsync(string text, bool isAppend, int? stop = null)
        {
            if (!isAppend)
            {
                contentText.text = string.Empty;
                contentText.maxVisibleCharacters = 99999;
                curCharacterCount = 0;

                if (string.IsNullOrEmpty(text))
                {
                    return Task.CompletedTask;
                }
            }

            contentText.text += text;
            contentText.maxVisibleCharacters = curCharacterCount;
            var cts = new TaskCompletionSource<object>();

            GameRoot.Timer.GetTimer<UpdateUntilTimer>().Add(OnUpdate, cts);

            if (stop.HasValue)
                this.stop = stop.Value;

            return cts.Task;
        }

        private bool OnUpdate(float deltaTime, object userdata)
        {
            var cts = userdata as TaskCompletionSource<object>;
            if (cts?.Task.IsCompleted ?? true) return true;

            remainingDeltaTime += deltaTime;
            float waitTime = (stop < 0 ? settingsModule.Stop : stop) / 1000f;

            if (remainingDeltaTime < waitTime) return false;

            remainingDeltaTime = 0;
            curCharacterCount += 1;
            contentText.maxVisibleCharacters = curCharacterCount;

            if (curCharacterCount >= contentText.textInfo.characterCount)
            {
                cts.SetResult(null);
                stop = -1;
                return true;
            }

            return false;
        }
    }
}
