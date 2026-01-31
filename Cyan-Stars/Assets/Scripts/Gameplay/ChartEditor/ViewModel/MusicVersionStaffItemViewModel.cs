#nullable enable

using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionStaffItemViewModel : BaseViewModel
    {
        private readonly MusicVersionViewModel MusicVersionViewModel;
        private readonly KeyValuePair<string, List<string>> StaffData;

        private readonly ReactiveProperty<string> name;
        public ReadOnlyReactiveProperty<string> Name => name;
        public IReadOnlyCollection<string> Jobs => StaffData.Value.AsReadOnly();


        public MusicVersionStaffItemViewModel(ChartEditorModel model,
                                              MusicVersionViewModel musicVersionViewModel,
                                              KeyValuePair<string, List<string>> staffData)
            : base(model)
        {
            MusicVersionViewModel = musicVersionViewModel;
            StaffData = staffData;

            name = new ReactiveProperty<string>(StaffData.Key); // 此处的观察用于强制刷新，直接赋初始值并在需要强制刷新时手动刷新。更新 StaffItem 时直接消耗重建
        }

        public void UpdateName(string newName)
        {
            if (newName == Name.CurrentValue)
                return;

            if (!MusicVersionViewModel.CheckNewStaffNameAvailable(newName))
            {
                name.ForceNotify();
                return;
            }

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
