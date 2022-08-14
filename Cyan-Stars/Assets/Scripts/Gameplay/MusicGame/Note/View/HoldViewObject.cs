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

        public void SetLength(float length)
        {
            var t = transform;
            var s = t.localScale;
            s.z = length;
            t.localScale = s;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            block.SetFloat(Flicker, 0);
            this.meshRenderer.SetPropertyBlock(block);
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
    }
}
