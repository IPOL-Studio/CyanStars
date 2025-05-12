using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class NoteHitEffect : MonoBehaviour
    {
        public bool WillDestroy;
        public float DestroyTime;
        public List<ParticleSystem> ParticleSystemList;
    }
}
