using CyanStars.Framework;
using CyanStars.Framework.Timeline;


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
        public static readonly CreateClipFunc<NoteTrack, NoteTrackData, NoteLayerData> CreateClipFunc = CreateClip;

        private static BaseClip<NoteTrack> CreateClip(NoteTrack track, NoteTrackData trackData, int curIndex, NoteLayerData _)
        {
            NoteClip clip = new NoteClip(0, GameRoot.GetDataModule<MusicGameModule>().CurTimelineLength, track,
                trackData.BaseSpeed, trackData.SpeedRate);

            for (int i = 0; i < trackData.LayerDatas.Count; i++)
            {
                //创建图层
                NoteLayerData layerData = trackData.LayerDatas[i];
                NoteLayer layer = new NoteLayer(layerData);

                for (int j = 0; j < layerData.TimeAxisDatas.Count; j++)
                {
                    NoteTimeAxisData timeAxisData = layerData.TimeAxisDatas[j];
                    for (int k = 0; k < timeAxisData.NoteDatas.Count; k++)
                    {
                        //创建音符
                        NoteData noteData = timeAxisData.NoteDatas[k];
                        BaseNote note = CreateNote(noteData, layer);
                        layer.AddNote(note);
                    }
                }

                clip.AddLayer(layer);
            }

            return clip;
        }


        /// <summary>
        /// 根据音符数据创建音符
        /// </summary>
        private static BaseNote CreateNote(NoteData noteData, NoteLayer layer)
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

            note.Init(noteData, layer);
            return note;
        }

        public void OnInput(InputEventArgs e)
        {
            if (Clips.Count == 0)
            {
                return;
            }

            NoteClip clip = (NoteClip)Clips[0];
            clip.OnInput(e);
        }
    }
}
