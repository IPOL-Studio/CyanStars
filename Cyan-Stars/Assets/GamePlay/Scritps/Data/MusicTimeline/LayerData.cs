using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐时间轴图层数据
/// </summary>
[System.Serializable]
public class LayerData
{

    /// <summary>
    /// 片段数据
    /// </summary>
    public List<ClipData> ClipDatas;

    /// <summary>
    /// 音符数据
    /// </summary>
    public List<NoteData> NoteDatas;
    
   
}
