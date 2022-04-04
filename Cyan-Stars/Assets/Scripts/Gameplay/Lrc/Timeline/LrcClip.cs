using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.Lrc.Timeline
{
    /// <summary>
    /// Lrc歌词片段
    /// </summary>
    public class LrcClip : BaseClip<LrcTrack>
    {

        private string lrcText;

        public LrcClip(float startTime, float endTime, LrcTrack owner, string lrcText) : base(startTime, endTime, owner)
        {
            this.lrcText = lrcText;
        }

        public override void OnEnter()
        {
            //显示歌词到UI上
            GameManager.Instance.curLrcText = lrcText;
        }
    }
}