using UnityEngine;
using CyanStars.Framework.Utils;

namespace CyanStars.Gameplay.MusicGame
{
    public class HoldViewObject : ViewObject
    {
        private static readonly int Flicker = Shader.PropertyToID("_Flicker");
        [SerializeField]
        private MeshRenderer meshRenderer;

        private MaterialPropertyBlock block = null;

        /// <summary>
        /// 是否被按下（在判定线上截断）
        /// </summary>
        public bool Pressed = false;

        /// <summary>
        /// 记录上一次截断时Hold的实际的Distance，在松开时将distance减去这个值修正位置
        /// </summary>
        private float lastDistanceWhenPressed = 0;


        public void SetPressed(bool pressed)
        {
            this.Pressed = pressed;
            OnUpdate(ViewDistance);
        }

        public override void OnUpdate(float viewDistance)
        {
            ViewDeltaTime = this.ViewDistance - viewDistance;
            this.ViewDistance = viewDistance;
            Vector3 pos = transform.position;

            if (Pressed)
            {
                // 视觉上的distance被设置为0（截断）
                pos.z = viewDistance > 0 ? ViewDistance : 0;
                lastDistanceWhenPressed = viewDistance;
            }
            else
            {
                // 按下时实际的distance为lastDistanceWhenPressed，减去这个值修正
                pos.z = viewDistance - lastDistanceWhenPressed;
            }

            transform.position = pos;
        }

        public void SetLength(float length)
        {
            var t = transform;
            var s = t.localScale;
            s.z = length;
            t.localScale = s;
        }

        protected override void Awake()
        {
            base.Awake();
            block = new MaterialPropertyBlock();
            block.SetFloat(Flicker, 0);
            meshRenderer.SetPropertyBlock(block);
        }


        private void OnEnable()
        {
            block.SetFloat(Flicker, 0);
            if (meshRenderer)
            {
                this.meshRenderer.SetPropertyBlock(block);
            }
        }

        public void OpenFlicker()
        {
            block.SetFloat(Flicker, 1.2f);
            if (meshRenderer)
            {
                this.meshRenderer.SetPropertyBlock(block);
            }
        }

        public override void DestroySelf(bool autoMove = true)
        {
            // 将对象归还到对象池之前重置为未按下状态
            Pressed = false;
            lastDistanceWhenPressed = 0;
            base.DestroySelf(autoMove);
        }
    }
}
