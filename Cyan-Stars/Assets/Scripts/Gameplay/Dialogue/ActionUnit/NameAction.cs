using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Event;
using Newtonsoft.Json;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueActionUnit("SetName")]
    public class NameAction : BaseActionUnit
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("bold")]
        public bool Bold { get; set; }

        [JsonProperty("italic")]
        public bool Italic { get; set; }

        public override void OnInit()
        {
            GameRoot.Event.Dispatch(EventConst.SetNameTextEvent, this, SingleEventArgs<string>.Create(CreateFinalText()));
            IsCompleted = true;
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnComplete()
        {
            IsCompleted = true;
        }

        private string CreateFinalText()
        {
            if (string.IsNullOrWhiteSpace(Text) || string.IsNullOrEmpty(Text))
            {
                return string.Empty;
            }

            bool colorSupport = ColorUtility.TryParseHtmlString(Color, out _);

            string colorLeft = colorSupport ? $"<color={Color}>" : string.Empty;
            string colorRight = colorSupport ? "</color>" : string.Empty;

            string boldLeft = Bold ? "<b>" : string.Empty;
            string boldRight = Bold ? "</b>" : string.Empty;

            string italicLeft = Italic ? "<i>" : string.Empty;
            string italicRight = Italic ? "</i>" : string.Empty;

            return $"{colorLeft}{boldLeft}{italicLeft}{Text}{italicRight}{boldRight}{colorRight}";
        }
    }
}
