/* 对于高度已知且开启了自动字体大小的单行 TMP_Text，总是会返回一个很大的 Preferred 宽度，尤其是考虑到宽度不一致的本地化 + UI 缩放等情况，这很烦人。
 * 现在我们让 Unity 先确定高度，TMP 再计算 AutoSize，最后用这个脚本强制调整 TMP_Text 的 RectTransform 宽度。
 *
 * 用法：将此脚本挂载到带有 TMP_Text 组件的物体上，将其 TextWrappingMode 设为 NoWrap，随后会自动根据物体高度和 AutoSize 来实时调整物体宽度。
 * 如果同时拖入 layoutElement，还会一并修改 layoutElement.preferredWidth，并在更新时立刻强制刷新布局
 */


#nullable enable

using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame.UI.ScoreSettlement
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    [RequireComponent(typeof(RectTransform))]
    public class TMPAutoWidthBox : MonoBehaviour
    {
        [Header("[可选] 覆盖此 LayoutElement 的 PreferredWidth")]
        [SerializeField]
        private LayoutElement? overrideLayoutElement;

        private TMP_Text text = null!;
        private RectTransform rectTransform = null!;

        // 缓存状态，用于检测是否需要刷新
        private string lastText = "";
        private float lastHeight = 0f;
        private Vector3 lastScale = Vector3.zero;
        private Vector2 lastScreenSize = Vector2.zero;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            RecordState();
            AdjustWidth();
        }

        private void LateUpdate()
        {
            if (text == null || rectTransform == null) return;

            // 如果文本、UI高度、全局缩放、或屏幕分辨率发生任何变化，立即重新计算
            if (HasStateChanged())
            {
                AdjustWidth();
                RecordState();
            }
        }

        // 检测状态是否发生改变
        private bool HasStateChanged()
        {
            // 1. 检测文本内容变化
            if (text.text != lastText) return true;

            // 2. 检测 UI 自身高度变化（可能由于锚点或父节点拉伸引起）
            if (Mathf.Abs(rectTransform.rect.height - lastHeight) > 0.001f) return true;

            // 3. 检测全局缩放变化（通常由 Canvas Scaler 引起）
            if (rectTransform.lossyScale != lastScale) return true;

            // 4. 检测屏幕分辨率变化
            Vector2 currentScreen = new Vector2(Screen.width, Screen.height);
            if (currentScreen != lastScreenSize) return true;

            return false;
        }

        // 记录当前状态
        private void RecordState()
        {
            lastText = text.text;
            lastHeight = rectTransform.rect.height;
            lastScale = rectTransform.lossyScale;
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }

        private void AdjustWidth()
        {
            // 1. 先给一个极大的宽度，让 AutoSize 能够完全根据高度来计算出正确的字体大小
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 999999f);

            // 2. 强制 TMP 立即刷新并计算网格
            text.ForceMeshUpdate();

            // 3. 获取实际渲染出来的文本宽度（基于刚刚算出来的真实字体大小）
            float actualWidth = text.GetRenderedValues(false).x;
            float margin = text.margin.x + text.margin.z;
            float targetWidth = actualWidth + margin;

            // 4. 如果有 LayoutElement，则修改它的 preferredWidth
            if (overrideLayoutElement != null)
            {
                overrideLayoutElement.preferredWidth = targetWidth;
            }

            // 5. 无论有没有 LayoutElement，手动设置一次 Rect 宽度保证非布局环境也正常
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

            // 6. 如果在布局组中，强制触发父级布局刷新
            if (overrideLayoutElement != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }
    }
}
