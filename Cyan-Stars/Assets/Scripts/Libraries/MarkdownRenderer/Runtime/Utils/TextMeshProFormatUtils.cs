using System;
using System.Linq;
using System.Threading.Tasks;

namespace CyanStars.MarkdownRenderer.Utils
{
    internal static class TextMeshProFormatUtils
    {
        public static string FormatNumber<T>(T value) where T : IFormattable =>
            value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    }
}