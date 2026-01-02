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

        public string Name => StaffData.Key;
        public IReadOnlyCollection<string> Jobs => StaffData.Value.AsReadOnly();


        public MusicVersionStaffItemViewModel(
            ChartEditorModel model, CommandManager commandManager,
            MusicVersionViewModel musicVersionViewModel, KeyValuePair<string, List<string>> staffData)
            : base(model, commandManager)
        {
            MusicVersionViewModel = musicVersionViewModel;
            StaffData = staffData;
        }

        public void UpdateName(string newName)
        {
            if (newName == Name)
                return;

            MusicVersionViewModel.RebuildStaffItemData(StaffData, new KeyValuePair<string, List<string>>(newName, StaffData.Value));
        }

        public void UpdateJob(string newJobString)
        {
            if (newJobString == string.Join('/', StaffData.Value))
                return;

            List<string> newJobs = new List<string>(newJobString.Split('/'));
            MusicVersionViewModel.RebuildStaffItemData(StaffData, new KeyValuePair<string, List<string>>(StaffData.Key, newJobs));
        }

        public void DeleteItem()
        {
            MusicVersionViewModel.DeleteStaffItemData(StaffData);
        }
    }
}
