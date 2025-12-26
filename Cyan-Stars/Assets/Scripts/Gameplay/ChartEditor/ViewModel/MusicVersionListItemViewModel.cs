#nullable enable

using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionListItemViewModel : BaseViewModel
    {
        private readonly MusicVersionViewModel MusicVersionViewModel;
        private readonly MusicVersionDataEditorModel MusicVersionData;

        public readonly ReadOnlyReactiveProperty<bool> IsSelected;
        public readonly ReadOnlyReactiveProperty<string> MusicItemTitle;


        public MusicVersionListItemViewModel(ChartEditorModel model, CommandManager commandManager,
                                             MusicVersionViewModel musicVersionViewModel, MusicVersionDataEditorModel musicVersionData)
            : base(model, commandManager)
        {
            MusicVersionViewModel = musicVersionViewModel;
            MusicVersionData = musicVersionData;

            IsSelected = MusicVersionViewModel.SelectedMusicVersionData
                .Select(selectedMusicVersion => selectedMusicVersion == MusicVersionData)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            MusicItemTitle = MusicVersionData.VersionTitle
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
        }

        public void OnToggleValueChanged(bool isOn)
        {
            if (!isOn)
                return; // 为 false 时代表 Unity 的 Toggle Group 自动取消选中，无需操作
            MusicVersionViewModel.SelectEditingMusicVersionData(MusicVersionData);
        }
    }
}
