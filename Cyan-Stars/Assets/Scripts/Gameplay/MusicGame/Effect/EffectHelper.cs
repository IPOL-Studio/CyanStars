using UnityEngine;
using System.Collections.Generic;

namespace CyanStars.Gameplay.MusicGame
{
    public class EffectHelper : MonoBehaviour
    {
        public static EffectHelper Instance;

        public GameObject Line;
        public GameObject BlockRain;

        public Dictionary<VfxType, List<string>> ParameterDict;

        private void Start()
        {
            Instance = this;
        }

        public GameObject GetAudioClipWithType(VfxType type) => type switch
        {
            VfxType.Line => Line,
            VfxType.BlockRain => BlockRain,
            _ => null
        };

        public List<string> GetParameterWithType(VfxType type) => type switch
        {
            VfxType.Line => new List<string> { "Count", "Color" },
            _ => throw new System.NotImplementedException()
        };
    }
}
