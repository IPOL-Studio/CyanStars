using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class HoldViewObject : ViewObject
    {
        private static readonly int Flicker = Shader.PropertyToID("_Flicker");

        [SerializeField]
        private MeshRenderer meshRenderer;

        private MaterialPropertyBlock block;

        private HoldNote note;

        /// <summary>
        /// 是否被按下（在判定线上截断）
        /// </summary>
        private bool pressed;

        protected override void Awake()
        {
            base.Awake();
            block = new MaterialPropertyBlock();
            block.SetFloat(Flicker, 0);
            meshRenderer.SetPropertyBlock(block);
        }


        private void OnEnable()
        {
            pressed = false;

            block.SetFloat(Flicker, 0);
            if (meshRenderer)
            {
                meshRenderer.SetPropertyBlock(block);
            }

            CloseFlicker();
        }


        public void Init(HoldNote note)
        {
            this.note = note;
        }

        public void SetPressed(bool pressed)
        {
            this.pressed = pressed;
            if (pressed)
            {
                OpenFlicker();
            }
            else
            {
                CloseFlicker();
            }
        }

        public override void OnUpdate(float viewDistance)
        {
            if (pressed)
            {
                viewDistance = 0;
            }

            Vector3 p = transform.position;
            p.z = -viewDistance;
            transform.position = p;

            SetLength();
        }

        public void SetLength()
        {
            Vector3 s = transform.localScale;
            if (pressed)
            {
                s.z = -(note.EndViewDistance - 0);
            }
            else
            {
                s.z = -(note.EndViewDistance - note.CurViewDistance);
            }

            transform.localScale = s;
        }

        public void OpenFlicker()
        {
            block.SetFloat(Flicker, 1.2f);
            if (meshRenderer)
            {
                meshRenderer.SetPropertyBlock(block);
            }
        }

        public void CloseFlicker()
        {
            block.SetFloat(Flicker, 0);
            if (meshRenderer)
            {
                meshRenderer.SetPropertyBlock(block);
            }
        }
    }
}
