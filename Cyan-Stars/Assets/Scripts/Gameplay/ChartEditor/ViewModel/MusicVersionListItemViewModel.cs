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
        private readonly MusicVersionData MusicVersionData;

        private readonly BindableProperty<string> musicItemTitle;

        public IReadonlyBindableProperty<string> MusicItemTitle => musicItemTitle;


        public MusicVersionListItemViewModel(
            ChartEditorModel model, CommandManager commandManager,
            MusicVersionViewModel musicVersionViewModel, MusicVersionData musicVersionData
        )
            : base(model, commandManager)
        {
            MusicVersionViewModel = musicVersionViewModel;
            MusicVersionData = musicVersionData;

            musicItemTitle = new BindableProperty<string>(MusicVersionData.VersionTitle);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
