using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 内置谱面列表SO
    /// </summary>
    [CreateAssetMenu(menuName = "创建内置谱包列表SO文件")]
    public class InternalChartPackListSO : ScriptableObject
    {
        [Header("内置谱包列表")]
        public List<string> InternalCharts;
    }
}
