#nullable enable

using CyanStars.Utils.RadioButton;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor
{
    public class SettingCanvasRadioButtonItem : MonoBehaviour
    {
        [SerializeField]
        private Image iconImage = null!;

        [SerializeField]
        private Sprite checkSprite = null!;

        [SerializeField]
        private Sprite crossSprite = null!;

        [SerializeField]
        private RadioButtonItem radioButton = null!;

        public RadioButtonItem RadioButton => radioButton;

        public void SetIconSelectedSprite(bool selected) => iconImage.sprite = selected ? checkSprite : crossSprite;
    }
}
