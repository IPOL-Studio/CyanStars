using UnityEngine;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.MusicGame
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
        public static readonly CreateKeyClipFunc<PromptToneTrack, PromptToneTrackData, PromptToneClipData, PromptToneClip> CreateClipFunc =
            CreateClip;

        public static readonly CreateKeyFunc<PromptToneClip, NoteData, PromptToneKey> CreateKeyFunc = CreateKey;

        private static PromptToneClip CreateClip(PromptToneTrack track, PromptToneTrackData trackData,
            int curIndex, PromptToneClipData data)
        {
            var lastData = data.KeyDataList[data.KeyCount - 1];
            AudioClip lastPromptTone = PromptToneHelper.Instance.GetAudioClipWithType(lastData.PromptToneType);

            return new PromptToneClip(0, lastData.JudgeTime / 1000f + lastPromptTone.length, track);
        }

        private static PromptToneKey CreateKey(PromptToneClip owner, NoteData data)
        {
            AudioClip promptTone = PromptToneHelper.Instance.GetAudioClipWithType(data.PromptToneType);

            if (promptTone == null) return null;

            PromptToneKey key = new PromptToneKey(owner, data.JudgeTime / 1000f, promptTone);
            return key;
        }
    }
}
