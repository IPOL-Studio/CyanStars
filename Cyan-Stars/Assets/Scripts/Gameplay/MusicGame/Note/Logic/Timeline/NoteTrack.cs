using CyanStars.Framework;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符轨道
    /// </summary>
    public class NoteTrack : BaseTrack
    {
        /// <summary>
        /// 片段创建方法
        /// </summary>
        public static readonly CreateClipFunc<NoteTrack, NoteTrackData, ChartData> CreateClipFunc = CreateClip;

        private static BaseClip<NoteTrack> CreateClip(NoteTrack track, NoteTrackData trackData, int curIndex,
            ChartData chartData)
        {
            NoteClip clip = new NoteClip(0, GameRoot.GetDataModule<MusicGameModule>().CurTimelineLength, track);
            foreach (BaseChartNoteData noteData in chartData.Notes)
            {
                BaseNote baseNote = CreateNote(noteData, chartData, clip);
                clip.InsertNote(baseNote);
            }

            return clip;
        }


        /// <summary>
        /// 根据音符数据创建音符
        /// </summary>
        private static BaseNote CreateNote(BaseChartNoteData noteData, ChartData chartData, NoteClip clip)
        {
            BaseNote note = noteData.Type switch
            {
                NoteType.Tap => new TapNote(),
                NoteType.Hold => new HoldNote(),
                NoteType.Drag => new DragNote(),
                NoteType.Click => new ClickNote(),
                NoteType.Break => new BreakNote(),
                _ => null
            };

            note?.Init(noteData, chartData, clip);
            return note;
        }
    }
}
