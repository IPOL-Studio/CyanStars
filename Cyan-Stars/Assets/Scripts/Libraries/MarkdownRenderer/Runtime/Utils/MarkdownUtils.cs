#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using CyanStars.MarkdownRenderer.Extensions.AtParagraph;
using CyanStars.MarkdownRenderer.Renderers;
using Markdig;
using Markdig.Syntax;

namespace CyanStars.MarkdownRenderer.Utils
{
    public static partial class MarkdownUtils
    {
        private static MarkdownPipeline? defaultPipeline;

        private static MarkdownPipeline DefaultPipeline =>
            defaultPipeline ??= new MarkdownPipelineBuilder().UseCjkFriendlyEmphasis().Build();

        private static MarkdownPipeline? cysExtensionDefaultPipeline;

        private static MarkdownPipeline CysExtensionDefaultPipeline
        {
            get
            {
                if (cysExtensionDefaultPipeline is null)
                {
                    var builder = new MarkdownPipelineBuilder().UseCjkFriendlyEmphasis();
                    builder.Extensions.Add(new AtParagraphExtension());
                    cysExtensionDefaultPipeline = builder.Build();
                }
                return cysExtensionDefaultPipeline;
            }
        }

        public static MarkdownPipeline GetOrCreateDefaultPipeline(bool useCysExtension)
        {
            return useCysExtension ? CysExtensionDefaultPipeline : DefaultPipeline;
        }

        public static string ToTextMeshPro(string markdownText, bool useCysExtension = false, TextMeshProRenderConfig? config = null)
        {
            if (string.IsNullOrEmpty(markdownText))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(markdownText))
            {
                return markdownText;
            }

            var pipeline = GetOrCreateDefaultPipeline(useCysExtension);
            var document = Markdown.Parse(markdownText, pipeline);
            return ToTextMeshPro(document, config, pipeline);
        }

        public static string ToTextMeshPro(this MarkdownDocument document, bool useCysExtension = false, TextMeshProRenderConfig? config = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));

            var pipeline = GetOrCreateDefaultPipeline(useCysExtension);
            return ToTextMeshPro(document, config, pipeline);
        }

        private static string ToTextMeshPro(MarkdownDocument document, TextMeshProRenderConfig? config, MarkdownPipeline pipeline)
        {
            using var writer = new StringWriter();
            var renderer = new TextMeshProRenderer(writer);
            pipeline.Setup(renderer);

            renderer.Config = config ?? TextMeshProRenderConfig.DefaultConfig;
            renderer.ComputeConfig();
            renderer.Render(document);
            renderer.Writer.Flush();
            return renderer.Writer.ToString() ?? string.Empty;
        }


        public static void ToTextMeshPro(this MarkdownDocument document, TextWriter writer, bool useCysExtension = false, TextMeshProRenderConfig? config = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));
            _ = writer ?? throw new ArgumentNullException(nameof(writer));

            var pipeline = GetOrCreateDefaultPipeline(useCysExtension);
            var renderer = new TextMeshProRenderer(writer);
            pipeline.Setup(renderer);

            renderer.Config = config ?? TextMeshProRenderConfig.DefaultConfig;
            renderer.ComputeConfig();
            renderer.Render(document);
            writer.Flush();
        }


        public static IList<AtInfo> CollectCysAtInfo(string markdownText)
        {
            if (markdownText == null)
            {
                throw new ArgumentNullException(nameof(markdownText));
            }

            if (markdownText == string.Empty)
            {
                return new List<AtInfo>();
            }

            var document = Markdown.Parse(markdownText, GetOrCreateDefaultPipeline(true));
            return CollectCysAtInfo(document);
        }

        public static IList<AtInfo> CollectCysAtInfo(MarkdownDocument document)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));

            var atInfoList = new List<AtInfo>();
            CollectAtInfoUtils.CollectAtInfo(document, atInfoList);
            return atInfoList;
        }

        /// <summary>
        /// 开足够大的 list 进来，不然还是可能付出扩容的开销
        /// <para> array 应该转成 span 用 <see cref="CollectCysAtInfoNonAlloc(MarkdownDocument, Span{AtInfo})"/> </para>
        /// </summary>
        public static int CollectCysAtInfoNonAlloc(MarkdownDocument document, IList<AtInfo> list, bool isClearList = false)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));
            _ = list ?? throw new ArgumentNullException(nameof(list));

            if (isClearList)
            {
                list.Clear();
            }

            return CollectAtInfoUtils.CollectAtInfo(document, list);
        }

        public static int CollectCysAtInfoNonAlloc(MarkdownDocument document, Span<AtInfo> span)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));
            return CollectAtInfoUtils.CollectAtInfo(document, span);
        }
    }
}
