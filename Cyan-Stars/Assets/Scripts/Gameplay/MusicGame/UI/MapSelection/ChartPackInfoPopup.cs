#nullable enable

using CyanStars.Chart;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars
{
    public class ChartPackInfoPopup : MonoBehaviour
    {
        [SerializeField]
        private Button closePopupButton = null!;

        [SerializeField]
        private TMP_Text infoText = null!;

        public void SetInfoRawText(string rawText) => infoText.text = ChartPackInfoHelper.ToTmpText(rawText);

        private void OnEnable() => closePopupButton.onClick.AddListener(ClosePopup);
        private void OnDisable() => closePopupButton.onClick.RemoveListener(ClosePopup);
        private void ClosePopup() => this.gameObject.SetActive(false);
    }
}
