using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    public interface IKeyClipData<TKeyData>
    {
        int KeyCount { get; }
        IList<TKeyData> KeyDataList { get; }
    }
}
