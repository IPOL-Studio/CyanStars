#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    /// <summary>
    /// 谱包 LinkItem 子 VM
    /// </summary>
    public class ChartPackDataLinkItemViewModel : BaseViewModel
    {
        private readonly ChartPackDataViewModel ChartPackDataViewModel;

        public readonly ReadOnlyReactiveProperty<LinkIcon?> LinkIcon;
        public readonly ReadOnlyReactiveProperty<string> LinkTitle;
        public readonly ReadOnlyReactiveProperty<string> LinkUrl;

        public ChartPackDataLinkItemViewModel(ChartEditorModel model,
                                              ChartPackDataViewModel chartPackDataViewModel,
                                              ChartPackLinkDataEditorModel chartPackLinkData)
            : base(model)
        {
            ChartPackDataViewModel = chartPackDataViewModel;

            LinkIcon = chartPackLinkData.LinkIcon.AddTo(base.Disposables);
            LinkTitle = chartPackLinkData.LinkTitle.AddTo(base.Disposables);
            LinkUrl = chartPackLinkData.LinkUrl.AddTo(base.Disposables);
        }
    }
}
