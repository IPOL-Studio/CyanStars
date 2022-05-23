using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 轨道数据接口
    /// </summary>
    public interface ITrackData<TClipData>
    {
        /// <summary>
        /// 获取片段数量
        /// </summary>
        int ClipCount { get; }
        
        /// <summary>
        /// 获取片段数据列表
        /// </summary>
        List<TClipData> ClipDataList { get; }
        

    }
}