using UnityEngine;
using System.Collections.Generic;

namespace CyanStars.Gameplay.MusicGame
{
    public class NoteClickEffect : MonoBehaviour
    {
        public bool WillDestroy;
        public float DestroyTime;

        public List<ParticleSystem> ParticleSystemList;

        void Start()
        {
            //Debug.Log(transform.position);
            if (WillDestroy) Destroy(gameObject, DestroyTime);
        }

        void Update()
        {
            var position = transform.position;
            transform.position = new Vector3(position.x, position.y, 0);
        }
    }
}
