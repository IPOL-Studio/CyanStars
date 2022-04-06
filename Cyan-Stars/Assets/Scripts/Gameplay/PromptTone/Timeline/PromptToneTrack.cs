using UnityEngine;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Note;

namespace CyanStars.Gameplay.PromptTone
{
    /// <summary>
    /// 提示音轨道
    /// </summary>
    public class PromptToneTrack : BaseTrack
    {
        public AudioSource audioSource;

        /// <summary>
        /// 创建提示音轨道片段
        /// </summary>
        public static readonly IClipCreator<PromptToneTrack, IList<NoteData>> ClipCreator = new PromptToneClipCreator();
        
        private sealed class PromptToneClipCreator : IClipCreator<PromptToneTrack, IList<NoteData>>
        {
            public BaseClip<PromptToneTrack> CreateClip(PromptToneTrack track, int clipIndex, IList<NoteData> notes)
            {
                AudioClip promptTone = PromptToneHelper.Instance.GetAudioClipWithType(notes[clipIndex].PromptToneType);

                if (promptTone == null) return new PromptToneClip(0, 0, track, promptTone);

                PromptToneClip clip = new PromptToneClip(notes[clipIndex].StartTime / 1000f,
                    notes[clipIndex].StartTime / 1000f + promptTone.length, track, promptTone);
                return clip;
            }
        }
    }
}
