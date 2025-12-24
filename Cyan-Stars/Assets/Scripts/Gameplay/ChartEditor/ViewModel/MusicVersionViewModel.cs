#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.BindableProperty;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionViewModel : BaseViewModel
    {
        private readonly BindableProperty<bool> canvasVisibility;
        private readonly BindableProperty<bool> listVisibility;
        private readonly BindableProperty<bool> detailVisibility;

        public IReadonlyBindableProperty<bool> CanvasVisibility => canvasVisibility;
        public IReadonlyBindableProperty<bool> ListVisibility => listVisibility;
        public IReadonlyBindableProperty<bool> DetailVisibility => detailVisibility;

        public readonly ObservableCollection<MusicVersionListItemViewModel> ListItems;
        public readonly ObservableCollection<MusicVersionStaffItemViewModel> StaffItems;


        public MusicVersionViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            // 初始化静态数据
            canvasVisibility = new BindableProperty<bool>(Model.MusicVersionCanvasVisibility.Value);
            listVisibility = new BindableProperty<bool>(!Model.IsSimplificationMode.Value);
            detailVisibility = new BindableProperty<bool>(true);

            // 初始化动态 VM
            List<MusicVersionListItemViewModel> listItems = new List<MusicVersionListItemViewModel>();
            foreach (var t in Model.ChartPackData.MusicVersionDatas)
            {
                var itemViewModel = new MusicVersionListItemViewModel(model, commandManager, this, t);
                listItems.Add(itemViewModel);
            }

            ListItems = new ObservableCollection<MusicVersionListItemViewModel>(listItems);

            List<MusicVersionStaffItemViewModel> staffItems = new List<MusicVersionStaffItemViewModel>();
            if (Model.ChartPackData.MusicVersionDatas.Count > 0)
            {
                MusicVersionData defaultMusicVersionData = Model.ChartPackData.MusicVersionDatas[0];
                foreach (var staffData in defaultMusicVersionData.Staffs)
                {
                    var viewModel = new MusicVersionStaffItemViewModel(model, commandManager,
                        this, staffData);
                    staffItems.Add(viewModel);
                }
            }

            StaffItems = new ObservableCollection<MusicVersionStaffItemViewModel>(staffItems);

            // M->VM
            // TODO
        }

        public void CloseCanvas()
        {
            Model.MusicVersionCanvasVisibility.Value = false;
        }

        private void Refresh(ChartPackData newChartPackData)
        {
        }
    }
}
