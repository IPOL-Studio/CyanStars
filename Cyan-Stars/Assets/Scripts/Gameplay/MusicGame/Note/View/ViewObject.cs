using UnityEngine;
using System.Collections;
using CyanStars.Framework;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 视图层物体脚本
    /// </summary>
    public class ViewObject : MonoBehaviour, IView
    {
        [SerializeField]
        private GameObject effectPrefab;

        private float viewDistance;
        private float viewDeltaTime;

        private GameObject effectObj;
        public string PrefabName;

        /// <summary>
        /// HOLD案件的开启效果函数，在HoldViewObject中实现
        /// </summary>
        public virtual void OpenFlicker() {}

        public void OnUpdate(float viewDistance)
        {
            viewDeltaTime = this.viewDistance - viewDistance;
            this.viewDistance = viewDistance;
            Vector3 pos = transform.position;
            pos.z = viewDistance;
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

            foreach (var particle in effectObj.GetComponent<NoteClickEffect>().ParticleSystemList)
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
                GameRoot.GameObjectPool.ReleaseGameObject(PrefabName, gameObject);
                return;
            }

            StartCoroutine(AutoMove());
        }

        /// <summary>
        /// 自动移动一段距离然后销毁自己
        /// </summary>
        private IEnumerator AutoMove()
        {
            Transform trans = transform;
            float timer = 0;
            while (true)
            {
                yield return null;
                timer += Time.deltaTime;

                viewDistance -= viewDeltaTime;
                Vector3 pos = trans.position;
                pos.z = viewDistance;
                trans.position = pos;

                if (timer >= 1f)
                {
                    GameRoot.GameObjectPool.ReleaseGameObject(PrefabName, gameObject);
                    yield break;
                }
            }
        }
    }
}
