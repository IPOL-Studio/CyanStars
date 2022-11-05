using System.Text;

namespace CyanStars.Gameplay.Dialogue
{
    public static class RichTextExtension
    {
        public static void AppendFullTextTo(this RichText self, StringBuilder sb)
        {
            sb.Append(self.LeftAttributes).Append(self.Text).Append(self.RightAttributes);
        }

        public static string GetFullText(this RichText self)
        {
            return $"{self.LeftAttributes}{self.Text}{self.RightAttributes}";
        }
    }
}
