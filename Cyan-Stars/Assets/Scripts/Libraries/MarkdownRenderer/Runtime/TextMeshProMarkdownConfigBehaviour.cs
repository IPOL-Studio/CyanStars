using UnityEngine;

namespace CyanStars.MarkdownRenderer
{
    public class TextMeshProMarkdownConfigBehaviour : MonoBehaviour, ITextMeshProMarkdownConfigProvider
    {
        [SerializeField] private TextMeshProRenderConfig config = new(TextMeshProRenderConfig.DefaultConfig);

        public TextMeshProRenderConfig Config => config;

            private void Reset()
        {
            config = new(TextMeshProRenderConfig.DefaultConfig);
        }
    }
}