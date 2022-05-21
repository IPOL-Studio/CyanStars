using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.MapData;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 音符轨道
    /// </summary>
    public class NoteTrack : BaseTrack
    {
        /// <summary>
        /// 创建音符轨道片段
        /// </summary>
        public static readonly IClipCreator<NoteTrack, MapTimelineData> ClipCreator = new NoteClipCreator();

        private sealed class NoteClipCreator : IClipCreator<NoteTrack, MapTimelineData>
        {
            public BaseClip<NoteTrack> CreateClip(NoteTrack track, int clipIndex, MapTimelineData data)
            {

                NoteClip clip = new NoteClip(0, data.Time / 1000f, track,data.NoteTrackData.BaseSpeed, data.NoteTrackData.SpeedRate);

                for (int i = 0; i < data.NoteTrackData.LayerDatas.Count; i++)
                {
                    //创建图层
                    NoteLayerData layerData = data.NoteTrackData.LayerDatas[i];
                    NoteLayer layer = new NoteLayer();

                    for (int j = 0; j < layerData.TimeAxisDatas.Count; j++)
                    {
                        //创建时轴
                        NoteTimeAxisData timeAxisData = layerData.TimeAxisDatas[j];
                        layer.AddTimeSpeedRate(timeAxisData.StartTime / 1000f, timeAxisData.SpeedRate);

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
        }

        /// <summary>
        /// 根据音符数据创建音符
        /// </summary>
        private static BaseNote CreateNote(NoteData noteData, NoteLayer layer)
        {
            BaseNote note = null;
            switch (noteData.Type)
            {
                case NoteType.Tap:
                    note = new TapNote();
                    break;
                case NoteType.Hold:
                    note = new HoldNote();
                    break;
                case NoteType.Drag:
                    note = new DragNote();
                    break;
                case NoteType.Click:
                    note = new ClickNote();
                    break;
                case NoteType.Break:
                    note = new BreakNote();
                    break;
            }

            note.Init(noteData, layer);
            return note;
        }

        public void OnInput(InputType inputType, InputMapData.Item item)
        {
            if (clips.Count == 0)
            {
                return;
            }

            NoteClip clip = (NoteClip)clips[0];
            clip.OnInput(inputType, item);
        }
    }
}
