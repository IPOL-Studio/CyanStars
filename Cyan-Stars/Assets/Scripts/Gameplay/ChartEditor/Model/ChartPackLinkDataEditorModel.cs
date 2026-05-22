#nullable enable

using System.Diagnostics.Contracts;
using CyanStars.Chart;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    /// <summary>
    /// 在制谱器内使用的谱包链接数据数据类
    /// </summary>
    public class ChartPackLinkDataEditorModel
    {
        public readonly ReactiveProperty<LinkIcon?> LinkIcon;
        public readonly ReactiveProperty<string> LinkTitle;
        public readonly ReactiveProperty<string> LinkUrl;

        public ChartPackLinkDataEditorModel(ChartPackLinkData chartPackLinkData)
        {
            LinkIcon = new ReactiveProperty<LinkIcon?>(chartPackLinkData.LinkIcon);
            LinkTitle = new ReactiveProperty<string>(chartPackLinkData.LinkTitle);
            LinkUrl = new ReactiveProperty<string>(chartPackLinkData.LinkUrl);
        }

        /// <summary>
        /// 将制谱器的可观察数据转为常规数据，以用于序列化
        /// </summary>
        [Pure]
        public ChartPackLinkData ToChartPackLinkData()
        {
            var linkIcon = LinkIcon.CurrentValue;
            var linkTitle = LinkTitle.CurrentValue;
            var linkUrl = LinkUrl.CurrentValue;
            return new ChartPackLinkData(linkIcon, linkTitle, linkUrl);
        }
    }
}
