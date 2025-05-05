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
                BaseNoteR baseNote = CreateNote(noteData, chartData, clip);
                clip.InsertNote(baseNote);
            }


            // for (int i = 0; i < trackData.LayerDatas.Count; i++)
            // {
            //     //创建图层
            //     NoteLayerData layerData = trackData.LayerDatas[i];
            //     NoteLayer layer = new NoteLayer(layerData);
            //
            //     for (int j = 0; j < layerData.TimeAxisDatas.Count; j++)
            //     {
            //         NoteTimeAxisData timeAxisData = layerData.TimeAxisDatas[j];
            //         for (int k = 0; k < timeAxisData.NoteDatas.Count; k++)
            //         {
            //             //创建音符
            //             NoteData noteData = timeAxisData.NoteDatas[k];
            //             BaseNote note = CreateNote(noteData, layer);
            //             layer.AddNote(note);
            //         }
            //     }
            //
            //     clip.AddLayer(layer);
            // }


            return clip;
        }


        /// <summary>
        /// 根据音符数据创建音符
        /// </summary>
        private static BaseNoteR CreateNote(BaseChartNoteData noteData, ChartData chartData, NoteClip clip)
        {
            BaseNoteR note = noteData.Type switch
            {
                NoteType.Tap => new TapNoteR(),
                NoteType.Hold => new HoldNoteR(),
                NoteType.Drag => new DragNoteR(),
                NoteType.Click => new ClickNoteR(),
                NoteType.Break => new BreakNoteR(),
                _ => null
            };

            note?.Init(noteData, chartData, clip);
            return note;
        }
    }
}
