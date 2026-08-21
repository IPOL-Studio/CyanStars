#nullable enable

using System.Linq;
using CyanStars.MarkdownRenderer.Extensions.StaffAt;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using NUnit.Framework;

namespace CyanStars.MarkdownRenderer.Tests
{
    public class StaffAtParseTests
    {
        private MarkdownPipeline pipeline = null!;

        [SetUp]
        public void Setup()
        {
            var builder = new MarkdownPipelineBuilder();
            builder.Extensions.Add(new StaffAtExtension());
            pipeline = builder.Build();
        }

        [TestCase("[@test]", "@test")]
        [TestCase("[@test\nnewline]", "@test newline")]
        [TestCase(@"[@test \[ \]]", "@test [ ]")]
        public void Should_Parse_Only_StaffAt_Inline(string text, string expectedLabel)
        {
            var markdownText = text;
            var document = Markdown.Parse(markdownText, pipeline);
            var inlines = document.Descendants<StaffAtInline>().ToArray();
            Assert.IsNotNull(inlines);
            Assert.AreEqual(1, inlines.Length);
            var inline = inlines[0];
            Assert.AreEqual(expectedLabel, inline.Content.ToString());
            Assert.AreEqual(expectedLabel.Substring(1), inline.Staff.ToString());
        }

        [Test]
        public void Should_Parse_Multiple_StaffAt_Inlines()
        {
            var markdownText = "[@test1]\n[@test2]";
            var document = Markdown.Parse(markdownText, pipeline);
            var inlines = document.Descendants<StaffAtInline>().ToArray();
            Assert.IsNotNull(inlines);
            Assert.AreEqual(2, inlines.Length);
            Assert.AreEqual("@test1", inlines[0].Content.ToString());
            Assert.AreEqual("@test2", inlines[1].Content.ToString());
        }

        [Test]
        public void Should_Parse_Multiple_StaffAt_Inlines_Without_Delimiter()
        {
            var markdownText = "[@test1][@test2][@test3]";
            var document = Markdown.Parse(markdownText, pipeline);
            var inlines = document.Descendants<StaffAtInline>().ToArray();
            Assert.IsNotNull(inlines);
            Assert.AreEqual(3, inlines.Length);
            Assert.AreEqual("@test1", inlines[0].Content.ToString());
            Assert.AreEqual("@test2", inlines[1].Content.ToString());
            Assert.AreEqual("@test3", inlines[2].Content.ToString());
        }

        [Test]
        public void Should_Skip_LinkInline_When_Parse_Multiple_Maybe_Matched_Inlines()
        {
            var markdownText = "[@test1]\n[@test2][@link]()\n[@test3]";
            var document = Markdown.Parse(markdownText, pipeline);
            var inlines = document.Descendants<StaffAtInline>().ToArray();
            Assert.IsNotNull(inlines);
            Assert.AreEqual(3, inlines.Length);
            Assert.AreEqual("@test1", inlines[0].Content.ToString());
            Assert.AreEqual("@test2", inlines[1].Content.ToString());
            Assert.AreEqual("@test3", inlines[2].Content.ToString());

            var linkInlines = document.Descendants<LinkInline>().ToArray();
            Assert.IsNotNull(linkInlines);
            Assert.AreEqual(1, linkInlines.Length);
            Assert.AreEqual("@link", ((LiteralInline)linkInlines[0].FirstChild!).Content.ToString());
        }
    }
}
