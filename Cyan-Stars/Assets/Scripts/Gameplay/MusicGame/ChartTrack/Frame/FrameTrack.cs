using CyanStars.Chart;
using CyanStars.Framework.Timeline;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    public class FrameTrack : BaseTrack
    {
        public Image ImgFrame;


        public static readonly CreateClipFunc<FrameTrack, FrameTrackData, FrameClipData> CreateClipFunc = CreateClip;

        private static BaseClip<FrameTrack> CreateClip(FrameTrack track, FrameTrackData trackData, int curIndex,
                                                       FrameClipData frameClipData)
        {
            float startTime =
                BpmGroupHelper.CalculateTime(frameClipData.BpmGroup, frameClipData.FrameChartTrackData.StartBeat) / 1000f;
            float endTime =
                BpmGroupHelper.CalculateTime(frameClipData.BpmGroup, frameClipData.FrameChartTrackData.EndBeat) / 1000f;
            return new FrameClip(startTime, endTime, track,
                frameClipData.FrameChartTrackData.Type, frameClipData.FrameChartTrackData.Color,
                frameClipData.FrameChartTrackData.Intensity, frameClipData.FrameChartTrackData.Bpm,
                frameClipData.FrameChartTrackData.MinAlpha, frameClipData.FrameChartTrackData.MaxAlpha);
            ;
        }
    }
}
