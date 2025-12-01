using System.Collections;
using CyanStars.Framework;
using CyanStars.Framework.Timer;
using CyanStars.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 视图层物体脚本
    /// </summary>
    public class ViewObject : MonoBehaviour, IView
    {
        [SerializeField]
        private NoteType noteType;

        private GameObject hitEffectObj;
        private string hitEffectPrefabName;

        private string notePrefabName;

        private TimerCallback timerCallback;
        protected float ViewDeltaTime;

        protected float ViewDistance;

        protected virtual void Awake()
        {
            MusicGameModule dataModule = GameRoot.GetDataModule<MusicGameModule>();
            notePrefabName = dataModule.NotePrefabNameDict[noteType];
            hitEffectPrefabName = dataModule.HitEffectPrefabNameDict[noteType];
            timerCallback = ReleaseHitEffectObj;
        }

        public virtual void OnUpdate(float viewDistance)
        {
            ViewDeltaTime = ViewDistance - viewDistance;
            ViewDistance = viewDistance;
            Vector3 pos = transform.position;
            pos.z = -viewDistance;
            transform.position = pos;
        }

        public async void CreateEffectObj(float w)
        {
            if (hitEffectObj != null)
            {
                return;
            }

            hitEffectObj = await GameRoot.GameObjectPool.GetGameObjectAsync(hitEffectPrefabName, null);
            hitEffectObj.transform.position = new Vector3(transform.position.x + Endpoint.Instance.Length * w / 2,
                transform.position.y, 0);
            hitEffectObj.transform.rotation = transform.rotation;
            hitEffectObj.transform.SetParent(ViewHelper.EffectRoot);

            NoteHitEffect hitEffect = hitEffectObj.GetComponent<NoteHitEffect>();
            if (hitEffect.WillDestroy)
            {
                GameRoot.Timer.GetTimer<IntervalTimer>().Add(hitEffect.DestroyTime, timerCallback);
            }
        }

        public void DestroyEffectObj()
        {
            if (hitEffectObj == null)
            {
                return;
            }

            ReleaseHitEffectObj(null);
        }

        public void DestroySelf(bool autoMove = true)
        {
            if (!autoMove)
            {
                GameRoot.GameObjectPool.ReleaseGameObject(notePrefabName, gameObject);
                return;
            }

            StartCoroutine(AutoMove());
        }

        private void ReleaseHitEffectObj(object userdata)
        {
            GameRoot.GameObjectPool.ReleaseGameObject(hitEffectPrefabName, hitEffectObj);
            hitEffectObj = null;
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

                ViewDistance -= ViewDeltaTime;
                Vector3 pos = trans.position;
                pos.z = -ViewDistance;
                trans.position = pos;

                if (timer >= 1f)
                {
                    GameRoot.GameObjectPool.ReleaseGameObject(notePrefabName, gameObject);
                    yield break;
                }
            }
        }
    }
}
