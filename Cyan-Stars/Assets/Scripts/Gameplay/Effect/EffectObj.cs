using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

namespace CyanStars.Gameplay.Effect
{
    public class EffectObj : MonoBehaviour
    {
        public float destroyTime = -1;
        public VisualEffect visualEffect;
        public float visualEffectStartCount = -1;

        void Start()
        {
            visualEffect = GetComponent<VisualEffect>();
            if (visualEffectStartCount <= 0) visualEffectStartCount = visualEffect.GetFloat("Count");
            if (destroyTime > 0)
            {
                StartCoroutine(DestroySelf());
            }
        }

        IEnumerator DestroySelf()
        {
            yield return new WaitForSeconds(destroyTime);
            visualEffect.SendEvent("Stop");
            Destroy(gameObject, 10);
        }
    }
}
