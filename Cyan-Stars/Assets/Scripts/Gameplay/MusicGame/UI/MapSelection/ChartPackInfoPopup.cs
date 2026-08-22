#nullable enable

using CyanStars.MarkdownRenderer;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars
{
    public class ChartPackInfoPopup : MonoBehaviour
    {
        [SerializeField]
        private Button closePopupButton = null!;

        [SerializeField]
        private TextMeshProMarkdown infoTMPMarkdown = null!;

        public void SetInfoRawText(string rawText) => infoTMPMarkdown.Text = rawText;

        private void OnEnable() => closePopupButton.onClick.AddListener(ClosePopup);
        private void OnDisable() => closePopupButton.onClick.RemoveListener(ClosePopup);
        private void ClosePopup() => this.gameObject.SetActive(false);
    }
}
