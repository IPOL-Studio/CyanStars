using System.Collections.Generic;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.MusicGame
{
    public class PromptToneDataCollection : IKeyClipData<NoteData>
    {
        public IList<NoteData> KeyDataList { get; }
        public int KeyCount => KeyDataList.Count;

        public PromptToneDataCollection(IList<NoteData> datas)
        {
            this.KeyDataList = datas;
        }

    }
}
