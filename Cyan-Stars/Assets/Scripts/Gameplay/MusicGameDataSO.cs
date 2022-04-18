using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay
{
    /// <summary>
    /// 音游谱面数据SO
    /// </summary>
    [CreateAssetMenu(menuName = "创建音游谱面数据SO文件")]
    public class MusicGameDataSO : ScriptableObject
    {
        [Header("音游谱面数据")]
        public MusicGameData Data;
    }
}

