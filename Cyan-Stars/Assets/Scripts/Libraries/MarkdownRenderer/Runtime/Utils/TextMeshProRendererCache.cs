#nullable enable
using System.IO;
using CyanStars.MarkdownRenderer.Renderers;
using Markdig;
using Markdig.Helpers;

namespace CyanStars.MarkdownRenderer.Utils
{
    internal sealed class TextMeshProRendererCache : ObjectCache<TextMeshProRenderer>
    {
        private static readonly TextWriter DummyWriter = new StringWriter();

        public readonly MarkdownPipeline Pipeline;
        public readonly bool CustomWriter;

        public TextMeshProRendererCache(MarkdownPipeline pipeline, bool customWriter)
        {
            Pipeline = pipeline;
            CustomWriter = customWriter;
        }

        protected override TextMeshProRenderer NewInstance()
        {
            var textWriter = CustomWriter ? DummyWriter : new StringWriter();
            var instance = new TextMeshProRenderer(textWriter);
            Pipeline.Setup(instance);
            return instance;
        }

        protected override void Reset(TextMeshProRenderer instance)
        {
            instance.ResetRecordedProps();
            instance.Config = TextMeshProRenderConfig.DefaultConfig;

            if (CustomWriter)
            {
                instance.Writer = DummyWriter;
            }
            else
            {
                ((StringWriter)instance.Writer).GetStringBuilder().Clear();
            }
        }
    }

    internal readonly ref struct RentedTextMeshProRenderer
    {
        private readonly TextMeshProRendererCache Cache;
        public readonly TextMeshProRenderer Renderer;

        internal RentedTextMeshProRenderer(TextMeshProRendererCache cache, TextMeshProRenderer renderer)
        {
            Cache = cache;
            Renderer = renderer;
        }

        public void Dispose() => Cache.Release(Renderer);
    }

    internal struct TextMeshProRendererCacheGroup
    {
        public TextMeshProRendererCache? DefaultCache;
        public TextMeshProRendererCache? CustomWriterCache;
    }

    internal static class TextMeshProRendererCacheGroupExtensions
    {
        public static ref TextMeshProRendererCache? RefCache(this ref TextMeshProRendererCacheGroup group, bool customWriter) =>
            ref (customWriter ? ref group.CustomWriterCache : ref group.DefaultCache);
    }
}
