using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱面边框轨道基类
    /// </summary>
    [ChartTrack("CyanStarsFrame")]
    public class FrameChartTrackData : IChartTrackData
    {
        public FrameType Type { get; set; } // 缓动效果类型
        public Beat StartBeat { get; set; } // 起始节拍
        public Beat EndBeat { get; set; } // 结束节拍
        public float Bpm { get; set; } // 每次循环播放的间隔时间
        public Color Color { get; set; } // 边框颜色
        public float Intensity { get; set; } // 边框强度/图像大小
        public float MaxAlpha { get; set; } // 播放时最大透明度
        public float MinAlpha { get; set; } // 播放时最小透明度


        public FrameChartTrackData(FrameType type, Beat startBeat, Beat endBeat, float bpm, Color color,
            float intensity, float maxAlpha, float minAlpha)
        {
            Type = type;
            StartBeat = startBeat;
            EndBeat = endBeat;
            Bpm = bpm;
            Color = color;
            Intensity = intensity;
            MaxAlpha = maxAlpha;
            MinAlpha = minAlpha;
        }
    }

    public enum FrameType
    {
        Breath, // 呼吸
        Flash // 脉冲
    }
}
