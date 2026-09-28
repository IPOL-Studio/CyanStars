#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 把弹窗面板的屏幕矩形与背景图 alpha 同步给 <see cref="ChartEditorPopupBlur"/>，用作模糊遮罩
    /// </summary>
    /// <remarks>
    /// 由 <c>BasePopupView</c> 在运行时挂到弹窗 Canvas 上。<br/>
    /// 面板取预制体下名为 <c>BackGround</c> 的子节点，其贴图决定了模糊的实际范围。
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class ChartEditorPopupBlurMask : MonoBehaviour
    {
        private const string BackgroundNodeName = "BackGround";

        private RectTransform panel = null!;
        private Texture? panelMask;
        private Vector4 panelMaskUV = new(0f, 0f, 1f, 1f);
        private bool visible;

        private void Awake()
        {
            var background = transform.Find(BackgroundNodeName) as RectTransform;
            if (background == null)
            {
                Debug.LogWarning($"[{nameof(ChartEditorPopupBlurMask)}] 未找到名为 {BackgroundNodeName} 的子节点，将使用弹窗根节点作为模糊遮罩");
                background = (RectTransform)transform;
            }

            panel = background;

            var image = background.GetComponent<Image>();
            var sprite = image != null ? image.sprite : null;
            var texture = sprite != null ? sprite.texture : null;

            if (texture != null)
            {
                panelMask = texture;

                Rect textureRect = sprite!.textureRect;
                panelMaskUV = new Vector4(
                    textureRect.x / texture.width,
                    textureRect.y / texture.height,
                    textureRect.width / texture.width,
                    textureRect.height / texture.height
                );
            }
            else
            {
                Debug.LogWarning($"[{nameof(ChartEditorPopupBlurMask)}] 面板上没有可用的 Sprite，背景模糊将不会生效");
            }
        }

        /// <summary>
        /// 由弹窗 View 在开关时调用
        /// </summary>
        public void SetVisible(bool value)
        {
            visible = value;

            if (!value)
                ChartEditorPopupBlur.ClearPanel(this);
        }

        private void LateUpdate()
        {
            if (!visible)
                return;

            // 每帧同步，跟随弹窗的缩放/淡入动画
            ChartEditorPopupBlur.UpdatePanel(this, panel, panelMask, panelMaskUV);
        }
    }
}
