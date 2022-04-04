using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Input;

namespace CyanStars.Gameplay.Note.Logic.Timeline
{
    /// <summary>
    /// 音符片段
    /// </summary>
    public class NoteClip : BaseClip<NoteTrack>
    {
        /// <summary>
        /// 基础速度
        /// </summary>
        private float baseSpeed;

        /// <summary>
        /// 速率
        /// </summary>
        private float speedRate;

        /// <summary>
        /// 片段速度
        /// </summary>
        private float clipSpeed;

        /// <summary>
        /// 音符图层列表
        /// </summary>
        private List<NoteLayer> layers = new List<NoteLayer>();

        public NoteClip(float startTime, float endTime, NoteTrack owner, float baseSpeed, float speedRate) : base(
            startTime, endTime, owner)
        {
            this.baseSpeed = baseSpeed;
            this.speedRate = speedRate;
            clipSpeed = baseSpeed * speedRate;
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
                layer.Update(currentTime, previousTime, clipSpeed);
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