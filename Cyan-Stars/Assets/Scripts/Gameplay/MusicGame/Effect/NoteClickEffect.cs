using UnityEngine;
using System.Collections.Generic;

namespace CyanStars.Gameplay.MusicGame
{
    public class NoteClickEffect : MonoBehaviour
    {
        public bool WillDestroy;
        public float DestroyTime;

        public List<ParticleSystem> ParticleSystemList;

        private Transform trans;

        void Start()
        {
            //Debug.Log(transform.position);
            if (WillDestroy) Destroy(gameObject, DestroyTime);
            this.trans = transform;
        }

        void Update()
        {
            var position = trans.position;
            trans.position = new Vector3(position.x, position.y, 0);
        }
    }
}
