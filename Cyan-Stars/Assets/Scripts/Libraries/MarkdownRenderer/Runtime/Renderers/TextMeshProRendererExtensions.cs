#nullable enable

using Markdig.Helpers;

namespace CyanStars.MarkdownRenderer.Renderers
{
    public static class TextMeshProRendererExtensions
    {
        public static void WriteLiteral(this TextMeshProRenderer renderer, ref StringSlice text)
        {
            var span = text.AsSpan();
            if (span.Length == 1 && span[0] == '\\')
            {
                renderer.Write(@"\\");
            }
            else if (span.Length == 2 && span[0] == '\\' && span[1] == '\\')
            {
                renderer.Write(@"\\\\");
            }
            else
            {
                renderer.Write(ref text);
            }
        }
    }
}
