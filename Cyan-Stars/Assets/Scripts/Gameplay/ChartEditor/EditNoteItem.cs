#nullable enable

using CyanStars.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditNoteItem : MonoBehaviour
    {
        public RectTransform Rect { get; private set; } = null!;

        [SerializeField]
        private RectTransform? holdTailRect; // 仅 Hold 音符需要赋值拖拽

        // 沿用旧代码的坐标参数
        private const float NotePosScale = 802.5f;
        private const float NotePosOffset = -321f;
        private const float BreakLeftX = -468.8f;
        private const float BreakRightX = 468.8f;

        private void Awake()
        {
            Rect = GetComponent<RectTransform>();
            // 确保 Anchor 符合 Content (0.5, 0) 的布局
            Rect.anchorMin = new Vector2(0.5f, 0f);
            Rect.anchorMax = new Vector2(0.5f, 0f);
            Rect.pivot = new Vector2(0.5f, 0f); // 音符底部对齐
        }

        public void SetData(BaseChartNoteData data, float startY, float endY = 0)
        {
            float xPos = 0;

            // 1. 计算 X 轴位置
            switch (data.Type)
            {
                case NoteType.Tap:
                case NoteType.Drag:
                case NoteType.Click:
                case NoteType.Hold:
                    if (data is IChartNoteNormalPos normalNote)
                    {
                        xPos = normalNote.Pos * NotePosScale + NotePosOffset;
                    }

                    break;
                case NoteType.Break:
                    if (data is BreakChartNoteData breakNote)
                    {
                        xPos = breakNote.BreakNotePos == BreakNotePos.Left ? BreakLeftX : BreakRightX;
                    }

                    break;
            }

            // 2. 设置位置 (Y 轴向上延伸，直接设置 anchoredPosition)
            Rect.anchoredPosition = new Vector2(xPos, startY);

            // 3. 处理 Hold 长度
            if (data.Type == NoteType.Hold && holdTailRect != null)
            {
                // 长度 = 结束位置 - 开始位置 - 头部微调
                float length = Mathf.Max(0, endY - startY - 12.5f);
                holdTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length);
            }
        }
    }
}
