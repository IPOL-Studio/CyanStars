using System.Collections.Generic;
using CyanStars.Chart;
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
        public List<InternalChartPackItem> InternalChartPacks;
    }

    [System.Serializable]
    public struct InternalChartPackItem
    {
        [Header("谱包索引文件路径（Assets/...）")]
        public string ChartPackFilePath;

        [Header("内置谱定数（float）")]
        public ChartPackLevels Levels;
    }
}
