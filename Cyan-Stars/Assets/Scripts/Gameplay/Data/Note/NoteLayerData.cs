using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.Data
{
    /// <summary>
    /// 音符图层数据
    /// </summary>
    [System.Serializable]
    public class NoteLayerData
    {
        /// <summary>
        /// 音符时轴数据
        /// </summary>
        [Header("音符时轴数据")]
        public List<NoteTimeAxisData> TimeAxisDatas;
    }

}

