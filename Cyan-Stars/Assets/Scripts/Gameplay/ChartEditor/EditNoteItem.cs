#nullable enable

using CyanStars.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditNoteItem : MonoBehaviour
    {
        public RectTransform Rect { get; private set; } = null!;
        public RectTransform? HoldTailRect; // 仅 Hold 音符需要赋值

        private const float NotePosScale = 802.5f;
        private const float NotePosOffset = -321f;
        private const float BreakLeftX = -468.8f;
        private const float BreakRightX = 468.8f;

        private void Awake()
        {
            Rect = GetComponent<RectTransform>();
        }

        public void SetData(BaseChartNoteData data, float yPos, float holdEndYPos = 0)
        {
            // 1. 设置 Y 轴位置
            float xPos = 0;

            // 2. 设置 X 轴位置
            switch (data.Type)
            {
                case NoteType.Tap:
                case NoteType.Drag:
                case NoteType.Click:
                case NoteType.Hold:
                    var normalNote = (IChartNoteNormalPos)data;
                    xPos = normalNote.Pos * NotePosScale + NotePosOffset;
                    break;
                case NoteType.Break:
                    var breakNote = (BreakChartNoteData)data;
                    xPos = breakNote.BreakNotePos == BreakNotePos.Left ? BreakLeftX : BreakRightX;
                    break;
            }

            Rect.anchoredPosition = new Vector2(xPos, yPos);

            // 3. 特殊处理 Hold
            if (data.Type == NoteType.Hold && HoldTailRect != null)
            {
                float length = Mathf.Max(0, holdEndYPos - yPos - 12.5f); // 12.5f 是头部偏移微调，沿用旧代码
                HoldTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length);
            }
        }
    }
}
