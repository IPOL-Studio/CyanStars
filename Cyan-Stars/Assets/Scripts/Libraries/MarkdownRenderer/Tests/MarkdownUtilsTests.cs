using System;
using System.IO;
using CyanStars.MarkdownRenderer.Utils;
using Markdig;
using Markdig.Syntax;
using NUnit.Framework;

namespace CyanStars.MarkdownRenderer.Tests
{
    public class MarkdownUtilsTests
    {
        [Test]
        public void Should_Collect_AtInfo_With_CysExtension_Pipeline()
        {
            var markdownText = "[@User1]\n[@User2_1] [@User2_2]\n\n[@UserWithLink]()";
            var infos = MarkdownUtils.CollectCysAtInfo(markdownText);

            Assert.AreEqual(4, infos.Count);
            Assert.AreEqual("User1", infos[0].Content);
            Assert.AreEqual("User2_1", infos[1].Content);
            Assert.AreEqual("User2_2", infos[2].Content);
            Assert.AreEqual("UserWithLink", infos[3].Content);
        }

        [Test]
        public void Should_CollectAtInfo_Without_Delimiter_And_With_CysExtension_Pipeline()
        {
            var markdownText = "[@User1][@User2_1][@User2_2][@UserWithLink]()";
            var infos = MarkdownUtils.CollectCysAtInfo(markdownText);

            Assert.AreEqual(4, infos.Count);
            Assert.AreEqual("User1", infos[0].Content);
            Assert.AreEqual("User2_1", infos[1].Content);
            Assert.AreEqual("User2_2", infos[2].Content);
            Assert.AreEqual("UserWithLink", infos[3].Content);
        }

        [Test]
        public void Should_Not_Colored_At_Paragraph_With_Default_Pipeline()
        {
            var markdownText = "[@User1]\n\n[@User2_1] [@User2_2]";
            var config = new TextMeshProRenderConfig(TextMeshProRenderConfig.DefaultConfig)
            {
                FinishBlockBehavior = FinishBlockBehavior.None
            };
            var text = MarkdownUtils.ToTextMeshPro(markdownText, false, config);
            var expectedText = $"[@User1]\n[@User2_1] [@User2_2]";
            Assert.AreEqual(expectedText, text);
        }

        [Test]
        public void Should_Colored_At_Paragraph_With_CysExtension_Pipeline()
        {
            var markdownText = "[@User1]\n\n[@User2_1] [@User2_2]";
            var config = new TextMeshProRenderConfig(TextMeshProRenderConfig.DefaultConfig)
            {
                FinishBlockBehavior = FinishBlockBehavior.None
            };
            var text = MarkdownUtils.ToTextMeshPro(markdownText, true, config);
            var expectedText = $"<color=#{config.AtColorHex}><u>@User1</u></color>\n<color=#{config.AtColorHex}><u>@User2_1</u></color> <color=#{config.AtColorHex}><u>@User2_2</u></color>";
            Assert.AreEqual(expectedText, text);
        }

        [Test]
        public void To_TextMeshPro_Should_Throw_ArgumentNullException_When_Document_Is_Null()
        {
            MarkdownDocument document = null;
            Assert.Throws<ArgumentNullException>(() => MarkdownUtils.ToTextMeshPro(document));
        }

        [Test]
        public void To_TextMeshPro_Should_Null_Or_Empty_String_When_Input_Text_Is_Empty()
        {
            var markdownText = string.Empty;
            var result = MarkdownUtils.ToTextMeshPro(markdownText);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void To_TextMeshPro_Should_Not_Be_Null_With_Custom_Writer()
        {
            var markdownText = "**test**";
            using var writer = new StringWriter();
            var pipeline = MarkdownUtils.GetOrCreateDefaultPipeline(false);
            var document = Markdown.Parse(markdownText, pipeline);
            MarkdownUtils.ToTextMeshPro(document, writer, false);
            var result = writer.ToString();
            Assert.NotNull(result);
        }
    }
}
