using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    public interface IKeyClipData<TKeyData>
    {
        /// <summary>
        /// 获取 Key 数量
        /// </summary>
        int KeyCount { get; }

        /// <summary>
        /// 获取 Key 数据列表
        /// </summary>
        IList<TKeyData> KeyDataList { get; }
    }
}
