#nullable enable

using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionStaffItemViewModel : BaseViewModel
    {
        private readonly MusicVersionViewModel MusicVersionViewModel;
        private readonly KeyValuePair<string, List<string>> StaffData;

        public MusicVersionStaffItemViewModel(
            ChartEditorModel model, CommandManager commandManager,
            MusicVersionViewModel musicVersionViewModel, KeyValuePair<string, List<string>> staffData)
            : base(model, commandManager)
        {
            MusicVersionViewModel = musicVersionViewModel;
            StaffData = staffData;
        }
    }
}
