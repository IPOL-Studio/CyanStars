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
        public AudioSource audioSource;

        /// <summary>
        /// 创建提示音轨道片段
        /// </summary>
        public static readonly IClipCreatorForEach<PromptToneTrack, NoteData> ClipCreator = new PromptToneClipCreator();
        
        private sealed class PromptToneClipCreator : IClipCreatorForEach<PromptToneTrack, NoteData>
        {
            public BaseClip<PromptToneTrack> CreateClip(PromptToneTrack track, NoteData note)
            {
                AudioClip promptTone = PromptToneHelper.Instance.GetAudioClipWithType(note.PromptToneType);

                if (promptTone == null) return new PromptToneClip(0, 0, track, promptTone);

                PromptToneClip clip = new PromptToneClip(note.StartTime / 1000f,
                    note.StartTime / 1000f + promptTone.length, track, promptTone);
                return clip;
            }
        }
    }
}
