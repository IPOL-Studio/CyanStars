using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 输入映射数据
/// </summary>
[System.Serializable]
public class InputMapData
{
    /// <summary>
    /// 数据项
    /// </summary>
    [System.Serializable]
    public class Item
    {
        /// <summary>
        /// 按键
        /// </summary>
        public KeyCode key;
        
        /// <summary>
        /// 映射范围的最小值
        /// 对应NoteData中的Pos
        /// </summary>
        [Range(-1,4)]
        public float RangeMin;

        /// <summary>
        /// 映射范围的最大值
        /// 对应NoteData中的Pos
        /// </summary>
        [Range(-1,4)]
        public float RangeMax;
    }

    /// <summary>
    /// 数据项列表
    /// </summary>
    public List<Item> Items;
}
