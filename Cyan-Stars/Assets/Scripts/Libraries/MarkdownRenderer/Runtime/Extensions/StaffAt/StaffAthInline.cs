using System;
using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Extensions.StaffAt
{
    public sealed class StaffAtInline : LeafInline
    {
        public StringSlice Content;
        public StringSlice Staff;

        public StaffAtInline(StringSlice content)
        {
            Content = content;
            var staff = content;
            staff.SkipChar();
            Staff = staff;
        }

        public StaffAtInline(string content)
        {
            _ = content ?? throw new ArgumentNullException(nameof(content));
            if (content.Length < 2 || content[0] != '@')
            {
                throw new ArgumentException("Content must start with '@' and have at least one character after it.", nameof(content));
            }
            Content = new StringSlice(content);
            Staff = new StringSlice(content, 1, content.Length - 1);
        }
    }
}
