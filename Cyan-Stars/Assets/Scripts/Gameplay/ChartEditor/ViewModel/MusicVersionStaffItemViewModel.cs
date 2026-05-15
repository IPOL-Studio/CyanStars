#nullable enable

using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionStaffItemViewModel : BaseViewModel
    {
        private readonly MusicVersionViewModel MusicVersionViewModel;

        private readonly ReactiveProperty<string> name;
        public ReadOnlyReactiveProperty<string> Name => name;


        public MusicVersionStaffItemViewModel(ChartEditorModel model,
                                              MusicVersionViewModel musicVersionViewModel,
                                              string staffName)
            : base(model)
        {
            MusicVersionViewModel = musicVersionViewModel;

            // 此处的观察用于在输入非法值（重复值）时强制刷新。
            // 如果是更新值，直接删除重建对应的子 VM 和 V。
            name = new ReactiveProperty<string>(staffName);
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

            MusicVersionViewModel.RebuildStaffItemData(name.CurrentValue, newName);
        }

        public void DeleteItem()
        {
            MusicVersionViewModel.DeleteStaffItemData(name.CurrentValue);
        }
    }
}
