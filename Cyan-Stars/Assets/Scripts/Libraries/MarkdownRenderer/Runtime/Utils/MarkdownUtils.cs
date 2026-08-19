#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using CyanStars.MarkdownRenderer.Extensions.AtParagraph;
using Markdig;
using Markdig.Syntax;

namespace CyanStars.MarkdownRenderer.Utils
{
    public static partial class MarkdownUtils
    {
        private static MarkdownPipeline? defaultPipeline;
        private static MarkdownPipeline DefaultPipeline => defaultPipeline ??=
            new MarkdownPipelineBuilder().UseCjkFriendlyEmphasis().Build();

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

            var pipeline = useCysExtension ? CysExtensionDefaultPipeline : DefaultPipeline;
            var document = Markdown.Parse(markdownText, pipeline);
            return ToTextMeshPro(document, useCysExtension, config, pipeline);
        }

        public static string ToTextMeshPro(this MarkdownDocument document, bool useCysExtension = false, TextMeshProRenderConfig? config = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));

            var pipeline = useCysExtension ? CysExtensionDefaultPipeline : DefaultPipeline;
            return ToTextMeshPro(document, useCysExtension, config, pipeline);
        }

        private static string ToTextMeshPro(MarkdownDocument document, bool useCysExtension, TextMeshProRenderConfig? config, MarkdownPipeline pipeline)
        {
            using var rentedRenderer = RentTextMeshProRenderer(pipeline, useCysExtension);
            var renderer = rentedRenderer.Renderer;

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

            var pipeline = useCysExtension ? CysExtensionDefaultPipeline : DefaultPipeline;
            using var rentedRenderer = RentTextMeshProRenderer(pipeline, useCysExtension, writer);
            var renderer = rentedRenderer.Renderer;

            renderer.Config = config ?? TextMeshProRenderConfig.DefaultConfig;
            renderer.ComputeConfig();
            renderer.Render(document);
            writer.Flush();
        }


        public static IList<AtInfo>? CollectCysAtInfo(string markdownText)
        {
            if (string.IsNullOrEmpty(markdownText))
            {
                return null;
            }

            var document = Markdown.Parse(markdownText, CysExtensionDefaultPipeline);
            return CollectCysAtInfo(document);
        }

        public static IList<AtInfo>? CollectCysAtInfo(MarkdownDocument document)
        {
            if (document is null)
            {
                return null;
            }

            var atInfoList = new List<AtInfo>();
            CollectAtInfoUtils.CollectAtInfo(document, atInfoList);
            return atInfoList;
        }

        /// <summary>
        /// 开足够大的 list 进来，不然还是可能付出扩容的开销
        /// <para> array 应该转成 span 用 <see cref="CollectCysAtInfoNonAlloc(MarkdownDocument, Span{AtInfo})"/> </para>
        /// </summary>
        public static int CollectCysAtInfoNonAlloc(MarkdownDocument document, IList<AtInfo> list)
        {
            return CollectAtInfoUtils.CollectAtInfo(document, list);
        }

        public static int CollectCysAtInfoNonAlloc(MarkdownDocument document, Span<AtInfo> span)
        {
            return CollectAtInfoUtils.CollectAtInfo(document, span);
        }
    }

    // renderer caches
    static partial class MarkdownUtils
    {
        private static TextMeshProRendererCacheGroup defaultRendererCacheGroup = new();
        private static TextMeshProRendererCacheGroup cysRendererCacheGroup = new();

        private static RentedTextMeshProRenderer RentTextMeshProRenderer(MarkdownPipeline pipeline, bool useCysExtension, TextWriter? write = null)
        {
            ref var cacheGroup = ref (useCysExtension ? ref cysRendererCacheGroup : ref defaultRendererCacheGroup);
            var isUseCustomWrite = write is not null;
            ref var cache = ref cacheGroup.RefCache(isUseCustomWrite);
            cache ??= new TextMeshProRendererCache(pipeline, isUseCustomWrite);

            var renderer = cache.Get();
            if (isUseCustomWrite)
            {
                renderer.Writer = write!;
            }

            return new RentedTextMeshProRenderer(cache, renderer);
        }
    }
}
