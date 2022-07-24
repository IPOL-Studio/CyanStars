using System.Collections.Generic;
using CyanStars.Framework.Timeline;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符片段
    /// </summary>
    public class NoteClip : BaseClip<NoteTrack>
    {

        /// <summary>
        /// 谱面整体速度
        /// </summary>
        private float mapSpeed;

        /// <summary>
        /// 音符图层列表
        /// </summary>
        private List<NoteLayer> layers = new List<NoteLayer>();

        public NoteClip(float startTime, float endTime, NoteTrack owner, float baseSpeed, float speedRate) : base(
            startTime, endTime, owner)
        {
            mapSpeed = baseSpeed * speedRate;
        }

        /// <summary>
        /// 添加音符图层
        /// </summary>
        public void AddLayer(NoteLayer layer)
        {
            layers.Add(layer);
        }

        public override void OnUpdate(float currentTime, float previousTime)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                NoteLayer layer = layers[i];
                layer.Update(currentTime, previousTime, mapSpeed);
            }
        }

        public void OnInput(InputType inputType, InputMapData.Item item)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnInput(inputType, item);
            }
        }
    }
}
