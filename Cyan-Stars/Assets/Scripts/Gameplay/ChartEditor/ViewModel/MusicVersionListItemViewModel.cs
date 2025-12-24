#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.BindableProperty;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionListItemViewModel : BaseViewModel
    {
        private readonly MusicVersionViewModel MusicVersionViewModel;
        public MusicVersionData MusicVersionData { get; }

        private readonly BindableProperty<bool> isSelected;
        private readonly BindableProperty<string> musicItemTitle;

        public IReadonlyBindableProperty<bool> IsSelected => isSelected;
        public IReadonlyBindableProperty<string> MusicItemTitle => musicItemTitle;


        public MusicVersionListItemViewModel(
            ChartEditorModel model, CommandManager commandManager,
            MusicVersionViewModel musicVersionViewModel, MusicVersionData musicVersionData
        )
            : base(model, commandManager)
        {
            MusicVersionViewModel = musicVersionViewModel;
            MusicVersionData = musicVersionData;

            bool selected = MusicVersionViewModel.SelectedMusicVersionData.Value == MusicVersionData;
            isSelected = new BindableProperty<bool>(selected);
            musicItemTitle = new BindableProperty<string>(MusicVersionData.VersionTitle);

            MusicVersionViewModel.SelectedMusicVersionData.OnValueChanged += SetSelectStatus;
            Model.OnMusicVersionDataChanged += RefreshView;
        }

        /// <summary>
        /// 由 Model 通知属性值发生变化，需要立刻刷新
        /// </summary>
        private void RefreshView(MusicVersionData data)
        {
            if (data != MusicVersionData)
                return;
            musicItemTitle.Value = data.VersionTitle;
        }

        private void SetSelectStatus(MusicVersionData? selectedData)
        {
            isSelected.Value = (selectedData == MusicVersionData);
        }

        public void OnToggleValueChanged(bool isOn)
        {
            if (!isOn) return; // 为 false 时代表 Unity 的 Toggle Group 自动取消选中，无需操作
            MusicVersionViewModel.SelectEditingMusicVersionData(MusicVersionData);
        }

        public override void Dispose()
        {
            MusicVersionViewModel.SelectedMusicVersionData.OnValueChanged -= SetSelectStatus;
            Model.OnMusicVersionDataChanged -= RefreshView;
        }
    }
}
