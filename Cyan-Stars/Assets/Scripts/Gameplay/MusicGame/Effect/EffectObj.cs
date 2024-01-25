using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using CyanStars.Framework;

namespace CyanStars.Gameplay.MusicGame
{
    public class EffectObj : MonoBehaviour
    {
        public float DestroyTime = -1;
        public VisualEffect VisualEffect;
        public float VisualEffectStartCount = -1;

        public string EffectName;

        public void PlayEffect()
        {
            VisualEffect = GetComponent<VisualEffect>();
            VisualEffect.Play();
            if (VisualEffectStartCount <= 0) VisualEffectStartCount = VisualEffect.GetFloat("Count");
            if (DestroyTime > 0)
            {
                StartCoroutine(DestroySelf());
            }
        }

       IEnumerator DestroySelf()
        {
            yield return new WaitForSeconds(DestroyTime);
            VisualEffect.SendEvent("Stop");
            yield return new WaitForSeconds(10f);
            //Destroy(gameObject, 10);
            GameRoot.GameObjectPool.ReleaseGameObject(EffectName, gameObject);
        }
    }
}
