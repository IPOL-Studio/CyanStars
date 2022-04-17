using UnityEngine;
using System.Collections.Generic;

namespace CyanStars.Gameplay.Effect
{
    public class EffectHelper: MonoBehaviour
    {
        public static EffectHelper Instance;

        public GameObject line;
        public GameObject blockRain;

        public Dictionary<VfxType, List<string>> parameterDict;

        private void Start()
        {
            Instance = this;
        }

        public GameObject GetAudioClipWithType(VfxType type) => type switch
        {
            VfxType.Line => line,
            VfxType.BlockRain => blockRain,
            _ => null
        };

        public List<string> GetParmeterWithType(VfxType type) => type switch
        {
            VfxType.Line => new List<string>() {"Count","Color"},
            _ => throw new System.NotImplementedException()
        };
    }
}