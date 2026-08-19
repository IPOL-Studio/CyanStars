using CyanStars.MarkdownRenderer.Utils;
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
    }
}
