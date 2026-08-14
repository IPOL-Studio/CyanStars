using UnityEngine;

namespace CyanStars.MarkdownRenderer
{
    [CreateAssetMenu(menuName = "Cyan Stars/Markdown/TextMeshPro Markdown Config")]
    public class TextMeshProMarkdownConfigSO : ScriptableObject, ITextMeshProMarkdownConfigProvider
    {
        [SerializeField] private TextMeshProRenderConfig config = new(TextMeshProRenderConfig.DefaultConfig);
        public TextMeshProRenderConfig Config => config;
    }
}