using UnityEngine;

namespace CyanStars.MarkdownRenderer
{
    public class TextMeshProMarkdownConfigBehaviour : MonoBehaviour, ITextMeshProMarkdownConfigProvider
    {
        [SerializeField] private TextMeshProMarkdownConfig config = new(TextMeshProMarkdownConfig.DefaultConfig);

        public TextMeshProMarkdownConfig Config => config;

            private void Reset()
        {
            config = new(TextMeshProMarkdownConfig.DefaultConfig);
        }
    }
}