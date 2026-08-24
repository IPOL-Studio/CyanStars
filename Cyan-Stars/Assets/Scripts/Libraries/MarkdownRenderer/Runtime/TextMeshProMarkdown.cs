using CyanStars.MarkdownRenderer.Renderers;
using CyanStars.MarkdownRenderer.Extensions.StaffAt;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using Markdig;
using Markdig.Syntax;
using UnityEngine.Assertions;
using System.IO;
using Ipol.UnityEx;

namespace CyanStars.MarkdownRenderer
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [ExecuteAlways]
    public class TextMeshProMarkdown : UIBehaviour
    {
        private TextMeshProUGUI renderTarget;
        private TextMeshProRenderer toTextMeshProRenderer;

        private MarkdownPipeline pipeline;

        [SerializeField, TextArea(5, 10)] private string text;
        [SerializeField] private ObjectRef<ITextMeshProMarkdownConfigProvider> configProvider;

        private ITextMeshProMarkdownConfigProvider observedConfigProvider;
        private TextMeshProRenderConfig observedConfig;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                isDirty = true;
            }
        }

        private bool isDirty;


        [Space(5)]
        [SerializeField]
        private UnityEvent<MarkdownDocument> onDocumentParsed = new UnityEvent<MarkdownDocument>();

        public UnityEvent<MarkdownDocument> OnDocumentParsed => onDocumentParsed;

        protected override void Awake()
        {
            renderTarget = GetComponent<TextMeshProUGUI>();
            renderTarget.richText = true;

            CreateRequires();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            isDirty = true;

            // 在 OnEnable 时立即构建一次，避免启用后、Update 之前残留一帧上次的旧文本造成闪烁
            BuildTextMeshProRichText();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            CreateRequires();
            SetDirty();
        }
#endif

        private void CreateRequires()
        {
            // 在编辑器环境下确保始终创建新 writer 对象，以防意外更改导致 renderer 状态残留
            var writer = new StringWriter(new StringBuilder(512));
            toTextMeshProRenderer ??= new TextMeshProRenderer(writer);

            if (pipeline == null)
            {
                var pipelineBuilder = new MarkdownPipelineBuilder().UseCjkFriendlyEmphasis();
                pipelineBuilder.Extensions.Add(new StaffAtExtension());
                pipeline = pipelineBuilder.Build();
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            DetectConfigChanges();
#endif

            if (isDirty)
            {
                BuildTextMeshProRichText();
            }
        }

        public void SetDirty() => isDirty = true;

#if UNITY_EDITOR
        private void DetectConfigChanges()
        {
            var currentProvider = configProvider?.Value;
            var currentConfig = currentProvider?.Config ?? TextMeshProRenderConfig.DefaultConfig;
            if (currentConfig.Equals(observedConfig))
            {
                return;
            }

            observedConfigProvider = currentProvider;
            observedConfig = new TextMeshProRenderConfig(currentConfig);
            SetDirty();
        }
#endif

        private void BuildTextMeshProRichText()
        {
            isDirty = false;
            Assert.IsNotNull(renderTarget, "Render target is not assigned.");
            Assert.IsNotNull(toTextMeshProRenderer, "TextMeshPro text renderer is not assigned.");
            Assert.IsNotNull(pipeline, "Markdown pipeline is not assigned.");

            if (string.IsNullOrEmpty(text))
            {
                onDocumentParsed.Invoke(null);
                if (!string.IsNullOrEmpty(renderTarget.text))
                {
                    renderTarget.text = string.Empty;
                }
                return;
            }

            var document = Markdown.Parse(text, pipeline);
            onDocumentParsed.Invoke(document);

            var renderer = toTextMeshProRenderer;
            pipeline.Setup(renderer);

            var writer = (StringWriter)toTextMeshProRenderer.Writer;
            writer.GetStringBuilder().Clear();
            renderer.ResetRecordedProps();

            renderer.Config = configProvider?.Value?.Config ?? TextMeshProRenderConfig.DefaultConfig;
            renderer.ComputeConfig();
            renderer.Render(document);
            writer.Flush();

            var result = writer.ToString() ?? string.Empty;

            renderTarget.text = result;
        }
    }
}
