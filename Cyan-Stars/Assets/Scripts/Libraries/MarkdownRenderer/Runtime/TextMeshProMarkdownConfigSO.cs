using UnityEngine;

namespace CyanStars.MarkdownRenderer
{
    [CreateAssetMenu(menuName = "Cyan Stars/Markdown/TextMeshPro Markdown Config")]
    public class TextMeshProMarkdownConfigSO : ScriptableObject, ITextMeshProMarkdownConfigProvider
    {
        [SerializeField] private TextMeshProMarkdownConfig config = new(TextMeshProMarkdownConfig.DefaultConfig);
        public TextMeshProMarkdownConfig Config => config;
    }
}