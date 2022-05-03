using CyanStars.Framework;
using CyanStars.Framework.GameObjectPool;
using UnityEngine;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.Effect
{
    /// <summary>
    /// 粒子特效片段
    /// </summary>
    public class ParticleEffectClip : BaseClip<EffectTrack>
    {
        /// <summary>
        /// 粒子特效预制索引
        /// </summary>
        private int effectPrefabIndex;

        private Vector3 position;
        private Vector3 rotation;

        /// <summary>
        /// 粒子数量，小于等于0则使用默认值
        /// </summary>
        private int particleCount;

        /// <summary>
        /// 持续时间
        /// </summary>
        private float duration;

        public ParticleEffectClip(float startTime, float endTime, EffectTrack owner, int effectPrefabIndex,
            Vector3 position, Vector3 rotation, int particleCount, float duration) : base(startTime, endTime, owner)
        {
            this.effectPrefabIndex = effectPrefabIndex;
            this.position = position;
            this.rotation = rotation;
            this.particleCount = particleCount;
            this.duration = duration;
        }

        public override async void OnEnter()
        {
            // GameObject effectObj =
            //     Object.Instantiate(Owner.EffectNames[effectPrefabIndex], position, Quaternion.Euler(rotation));
            // effectObj.gameObject.transform.SetParent(Owner.EffectParent);

            GameObject effectGO =
                await GameRoot.GameObjectPool.AwaitGetGameObject(Owner.EffectNames[effectPrefabIndex]);
            effectGO.transform.SetParent(Owner.EffectParent);
            effectGO.transform.position = position;
            effectGO.transform.rotation = Quaternion.Euler(rotation);
           
            
            EffectObj eo = effectGO.GetComponent<EffectObj>();
            eo.effectName = Owner.EffectNames[effectPrefabIndex];
            eo.destroyTime = duration;
            eo.visualEffectStartCount = particleCount;
            eo.PlayEffect();
        }
    }
}
