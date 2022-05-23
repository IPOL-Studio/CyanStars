using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MapData
{
    /// <summary>
    /// 内置谱面列表SO
    /// </summary>
    [CreateAssetMenu(menuName = "创建内置谱面列表SO文件")]
    public class InternalMapListSO : ScriptableObject
    {
        [Header("内置谱面列表")]
        public List<MapManifest> InternalMaps;
    }
}
