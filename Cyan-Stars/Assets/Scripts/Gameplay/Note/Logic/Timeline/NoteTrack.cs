using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Camera;
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
        /// 片段创建方法
        /// </summary> 
        public static readonly CreateClipFunc<NoteTrack,NoteTrackData, NoteLayerData> CreateClipFunc = CreateClip;
        
        private static BaseClip<NoteTrack> CreateClip(NoteTrack track, NoteTrackData trackData, int curIndex, NoteLayerData _)
        {
            
            NoteClip clip = new NoteClip(0, GameRoot.GetDataModule<MusicGameModule>().CurTimelineLength, track,trackData.BaseSpeed, trackData.SpeedRate);

            for (int i = 0; i < trackData.LayerDatas.Count; i++)
            {
                //创建图层
                NoteLayerData layerData = trackData.LayerDatas[i];
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
            if (Clips.Count == 0)
            {
                return;
            }

            NoteClip clip = (NoteClip)Clips[0];
            clip.OnInput(inputType, item);
        }
    }
}
