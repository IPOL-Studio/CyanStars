using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 用于加载到片段的数据
    /// </summary>
    public class MusicClipData
    {
        public AudioClip audioClip;
        public int offset;

        public MusicClipData(AudioClip audioClip, int offset)
        {
            this.audioClip = audioClip;
            this.offset = offset;
        }
    }
}
