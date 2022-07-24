using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 输入映射数据的SO
    /// </summary>
    [CreateAssetMenu(menuName = "创建输入映射数据")]
    public class InputMapSO : ScriptableObject
    {
        /// <summary>
        /// 输入映射数据
        /// </summary>
        public InputMapData InputMapData;
    }
}