#nullable enable

using CyanStars.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditNoteItem : MonoBehaviour
    {
        public RectTransform Rect { get; private set; } = null!;

        [SerializeField]
        private RectTransform? holdTailRect; // 仅 Hold 音符需要赋值

        private const float NotePosScale = 802.5f;
        private const float NotePosOffset = -321f;
        private const float BreakLeftX = -468.8f;
        private const float BreakRightX = 468.8f;

        private void Awake()
        {
            Rect = GetComponent<RectTransform>();
            Rect.anchorMin = new Vector2(0.5f, 0f);
            Rect.anchorMax = new Vector2(0.5f, 0f);
            Rect.pivot = new Vector2(0.5f, 0.5f);
        }

        public void SetData(BaseChartNoteData data, float startY, float endY = 0)
        {
            float xPos = 0;

            // 计算 X 轴位置
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

            // 设置位置
            Rect.anchoredPosition = new Vector2(xPos, startY);

            // 处理 Hold 长度
            if (data.Type == NoteType.Hold && holdTailRect != null)
            {
                // 长度 = 结束位置 - 开始位置 - 头部微调
                float length = Mathf.Max(0, endY - startY - 12.5f);
                holdTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length);
            }
        }
    }
}
