using System.Collections.Generic;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.MusicGame
{
    public class PromptToneClipData : IKeyClipData<NoteData>
    {
        public IList<NoteData> KeyDataList { get; }
        public int KeyCount => KeyDataList.Count;

        public PromptToneClipData(IList<NoteData> datas)
        {
            this.KeyDataList = datas;
        }

    }
}
