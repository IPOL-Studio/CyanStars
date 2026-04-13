#nullable enable

using System;
using CyanStars.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    public static class EditAreaViewHelper
    {
        private const float DefaultMajorBeatLineInterval = 250f;
        private const float DefaultNoteWidth = 160f;

        private const float LeftBreakThreshold = -421f;
        private const float RightBreakThreshold = 421f;
        private const float CentralMin = -400f;
        private const float CentralMax = 400f;

        private const float MinMainTrackPos = 0f;
        private const float MaxMainTrackPos = 0.8f;
        private const float LeftBreakNotePos = -1f;
        private const float RightBreakNotePos = 2f;

        private const float CentralTrackWidth = CentralMax - CentralMin;


        /// <summary>
        /// 获取两条细分节拍线之间的像素距离
        /// </summary>
        public static double GetMinorBeatLineDistance(int beatAccuracy, double beatZoom)
        {
            return DefaultMajorBeatLineInterval * beatZoom / beatAccuracy;
        }

        /// <summary>
        /// 将点击位置转换到音符 pos 和 beat
        /// </summary>
        /// <param name="localPosition">以 Content 底部中心为原点，点击处的像素坐标</param>
        /// <param name="judgeLineY">以 Content 底部为 0，判定线的 y 坐标</param>
        /// <param name="isPosMagnetOn">是否开启了位置吸附</param>
        /// <param name="posAccuracy">当前的位置细分</param>
        /// <param name="beatAccuracy">当前的节拍细分</param>
        /// <param name="beatZoom">当前的编辑器内节拍缩放比例</param>
        /// <param name="pos">点击处对应的音符 pos [0f, 0.8f]（音符 pos 指的是以中央轨道左侧为 0，音符左端点在中央轨道上的位置比例；特别的，左 Break 为 -1，右 Break 为 2）</param>
        /// <param name="beat">点击处对应的音符 beat</param>
        /// <returns>
        /// 如果应该正常创建音符，返回 true；
        /// 否则如果点到了两条轨道之间的缝隙，则返回 false，此时 pos 和 beat 无意义，调用方应当丢弃此点击响应并且不创建任何音符
        /// </returns>
        public static bool CalculateNotePlacement(
            Vector2 localPosition,
            float judgeLineY,
            bool isPosMagnetOn,
            int posAccuracy,
            int beatAccuracy,
            double beatZoom,
            out float pos,
            out Beat beat
        )
        {
            pos = MinMainTrackPos;
            Beat.TryCreateBeat(0, 0, 1, out beat);

            // 计算 pos
            if (localPosition.x <= LeftBreakThreshold)
            {
                // Left Break
                pos = LeftBreakNotePos;
            }
            else if (RightBreakThreshold <= localPosition.x)
            {
                // Right Break
                pos = RightBreakNotePos;
            }
            else if (CentralMin <= localPosition.x && localPosition.x <= CentralMax)
            {
                if (isPosMagnetOn)
                {
                    // 开启了横坐标吸附
                    if (posAccuracy == 0)
                    {
                        // 居中放置
                        pos = MinMainTrackPos + (MaxMainTrackPos - MinMainTrackPos) / 2f;
                    }
                    else
                    {
                        // 计算每个细分半步长的宽度 (分母的 2 表示半步长)
                        float subSectionWidth = (CentralTrackWidth / (posAccuracy + 1)) / 2f;

                        // 将 X 坐标系从以中心为原点 (-400 ~ 400) 转换到以左边缘为原点 (0 ~ 800)
                        float relativePosX = localPosition.x - CentralMin;

                        // 计算吸附索引
                        float snappingIndex = Mathf.Round(relativePosX / subSectionWidth);
                        float maxIndex = 2 * posAccuracy + 1;
                        snappingIndex = Mathf.Clamp(snappingIndex, 1, maxIndex);

                        // 计算吸附后的中心点局部坐标，并转换回中心原点坐标系
                        float snappedRelativePos = snappingIndex * subSectionWidth;
                        float posX = snappedRelativePos + CentralMin;

                        // 计算吸附后音符左端点的 X 坐标
                        float noteLeftEdgeX = posX - DefaultNoteWidth / 2f;

                        // 换算成 Pos 比例 (相对于轨道左端点的距离 / 轨道总宽)
                        pos = (noteLeftEdgeX - CentralMin) / CentralTrackWidth;
                        pos = Mathf.Clamp(pos, MinMainTrackPos, MaxMainTrackPos);
                    }
                }
                else
                {
                    // 未开启位置吸附
                    // 计算音符左端点的 X 坐标
                    float noteLeftEdgeX = localPosition.x - DefaultNoteWidth / 2f;

                    // 换算成 Pos 比例
                    pos = (noteLeftEdgeX - CentralMin) / CentralTrackWidth;
                    pos = Mathf.Clamp(pos, MinMainTrackPos, MaxMainTrackPos);
                }
            }
            else
            {
                // 点击了缝隙
                return false;
            }

            // 计算 beat
            float relativeY = localPosition.y - judgeLineY;
            double beatDistance = GetMinorBeatLineDistance(beatAccuracy, beatZoom);

            int subBeatIndex = (int)Math.Round(relativeY / beatDistance);
            subBeatIndex = Mathf.Max(0, subBeatIndex);

            int acc = beatAccuracy;
            return Beat.TryCreateBeat(subBeatIndex / acc, subBeatIndex % acc, acc, out beat);
        }
    }
}
