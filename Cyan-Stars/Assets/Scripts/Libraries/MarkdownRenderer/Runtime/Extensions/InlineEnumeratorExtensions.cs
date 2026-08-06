using System.Collections.Generic;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer
{
    public static class ContainerInlineExtensions
    {
        public static IEnumerable<Inline> FindDescendants<T1, T2>(this ContainerInline container)
            where T1 : Inline
            where T2 : Inline
        {
            Stack<Inline> stack = new Stack<Inline>();
            for (Inline child = container.LastChild; child != null; child = child.PreviousSibling)
            {
                stack.Push(child);
            }

            while (stack.Count > 0)
            {
                Inline child = stack.Pop();
                if (child is T1 val1)
                {
                    yield return val1;
                }
                else if (child is T2 val2)
                {
                    yield return val2;
                }

                if (child is ContainerInline containerInline)
                {
                    for (child = containerInline.LastChild; child != null; child = child.PreviousSibling)
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }
}