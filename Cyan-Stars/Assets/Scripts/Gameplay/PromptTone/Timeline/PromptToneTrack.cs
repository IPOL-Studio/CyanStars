using UnityEngine;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Data;

namespace CyanStars.Gameplay.PromptTone
{
    /// <summary>
    /// 提示音轨道
    /// </summary>
    public class PromptToneTrack : BaseTrack
    {
        public AudioSource AudioSource;

        /// <summary>
        /// 片段创建方法
        /// </summary>
        public static readonly CreateClipFunc<PromptToneTrack, PromptToneTrackData, NoteData> CreateClipFunc =
            CreateClip;

        private static BaseClip<PromptToneTrack> CreateClip(PromptToneTrack track, PromptToneTrackData trackData,
            int curIndex, NoteData noteData)
        {
            AudioClip promptTone = PromptToneHelper.Instance.GetAudioClipWithType(noteData.PromptToneType);

            if (promptTone == null) return new PromptToneClip(0, 0, track, promptTone);

            PromptToneClip clip = new PromptToneClip(noteData.StartTime / 1000f,
                noteData.StartTime / 1000f + promptTone.length, track, promptTone);
            return clip;
        }
    }
}
