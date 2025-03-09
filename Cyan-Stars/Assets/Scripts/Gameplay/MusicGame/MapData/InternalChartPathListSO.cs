using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 内置谱面列表SO
    /// </summary>
    [CreateAssetMenu(menuName = "创建内置谱面路径SO文件")]
    public class InternalChartPathListSO : ScriptableObject
    {
        [Header("内置谱面路径")]
        public List<string> InternalChartPaths;
    }
}
