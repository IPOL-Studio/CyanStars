using System;
using UnityEngine;
using System.Collections;
using CyanStars.Framework;
using CyanStars.Gameplay.Effect;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 视图层物体脚本
    /// </summary>
    public class ViewObject : MonoBehaviour, IView
    {
        private float deltaTime;
        public GameObject effectPrefab;
        private GameObject effectObj;
        public string PrefabName;
        
        


        public void OnUpdate(float deltaTime)
        {
            this.deltaTime = deltaTime;

            Vector3 pos = transform.position;
            pos.z -= deltaTime;
            transform.position = pos;
        }

        public void CreateEffectObj(float w)
        {
            effectObj = GameObject.Instantiate(effectPrefab,
                transform.position + new Vector3(Endpoint.Instance.Length * w / 2, 0, 0), Quaternion.identity);
            effectObj.transform.SetParent(ViewHelper.EffectRoot);
        }

        public void DestroyEffectObj()
        {
            if (effectObj == null)
            {
                return;
            }

            foreach (var particle in effectObj.GetComponent<NoteClickEffect>().particleSystemList)
            {
                particle.Stop();
            }

            Destroy(effectObj, 5);
        }

        public void DestroySelf(bool autoMove = true)
        {
            if (!autoMove)
            {
                //Destroy(gameObject);
                GameRoot.GameObjectPool.ReleaseGameObject(PrefabName,gameObject);
                return;
            }

            StartCoroutine(AutoMove());
        }

        /// <summary>
        /// 自动移动一段时间然后销毁自己
        /// </summary>
        private IEnumerator AutoMove()
        {
            float timer = 0;
            var trans = transform;
            while (true)
            {
                timer += deltaTime;

                Vector3 pos = trans.position;
                pos.z -= deltaTime;
                trans.position = pos;

                if (timer >= 2f)
                {
                    //Destroy(gameObject);
                    GameRoot.GameObjectPool.ReleaseGameObject(PrefabName,gameObject);
                    yield break;
                }

                yield return null;
            }
        }
    }
}
