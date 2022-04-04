using UnityEngine;

namespace CyanStars.Gameplay.Input
{
    /// <summary>
    /// 输入映射数据的SO
    /// </summary>
    [CreateAssetMenu(menuName = "输入映射数据配置")]
    public class InputMapSO : ScriptableObject
    {
        /// <summary>
        /// 输入映射数据
        /// </summary>
        public InputMapData InputMapData;
    }
}