using System;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using CyanStars.Framework;

namespace CyanStars.Gameplay.Effect
{
    public class EffectObj : MonoBehaviour
    {
        public float destroyTime = -1;
        public VisualEffect visualEffect;
        public float visualEffectStartCount = -1;

        public string effectName;
        


        public void PlayEffect()
        {
            visualEffect = GetComponent<VisualEffect>();
            visualEffect.Play();
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
            yield return new WaitForSeconds(10f);
            //Destroy(gameObject, 10);
            GameRoot.GameObjectPool.ReleaseGameObject(effectName,gameObject);
        }
    }
}
