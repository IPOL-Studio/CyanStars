#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.BindableProperty;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionViewModel : BaseViewModel
    {
        private const int AddOffsetStep = 10;


        private readonly BindableProperty<MusicVersionData?> selectedMusicVersionData;
        public IReadonlyBindableProperty<MusicVersionData?> SelectedMusicVersionData => selectedMusicVersionData;


        private readonly BindableProperty<bool> canvasVisibility;
        private readonly BindableProperty<bool> listVisibility;
        private readonly BindableProperty<bool> detailVisibility;

        public IReadonlyBindableProperty<bool> CanvasVisibility => canvasVisibility;
        public IReadonlyBindableProperty<bool> ListVisibility => listVisibility;
        public IReadonlyBindableProperty<bool> DetailVisibility => detailVisibility;


        private readonly BindableProperty<string> detailTitle;
        private readonly BindableProperty<string> detailAudioFilePath;
        private readonly BindableProperty<string> detailOffset;

        public IReadonlyBindableProperty<string> DetailAudioFilePath => detailAudioFilePath;
        public IReadonlyBindableProperty<string> DetailOffset => detailOffset;
        public IReadonlyBindableProperty<string> DetailTitle => detailTitle;


        public readonly ObservableCollection<MusicVersionListItemViewModel> ListItems;
        public readonly ObservableCollection<MusicVersionStaffItemViewModel> StaffItems;


        public MusicVersionViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            // 初始化静态数据
            MusicVersionData? data = Model.ChartPackData.MusicVersionDatas.Count > 0
                ? Model.ChartPackData.MusicVersionDatas[0]
                : null;
            selectedMusicVersionData = new BindableProperty<MusicVersionData?>(data);
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
            Model.OnMusicVersionListChanged += RefreshList;
            Model.OnMusicVersionDataChanged += RefreshData;
        }

        private void RefreshList(IReadOnlyCollection<MusicVersionData> datas)
        {
            // 删除多余 VM
            var hashSet = new HashSet<MusicVersionData>(datas);
            for (int i = ListItems.Count - 1; i >= 0; i--)
            {
                if (!hashSet.Contains(ListItems[i].MusicVersionData))
                {
                    ListItems[i].Dispose();
                    ListItems.RemoveAt(i);
                }
            }

            // 排序既有 VM，新增 VM
            int index = 0;
            foreach (var data in datas)
            {
                var existingViewModel = ListItems.FirstOrDefault(vm => vm.MusicVersionData == data);

                if (existingViewModel != null)
                {
                    // VM 已存在，排序
                    int oldIndex = ListItems.IndexOf(existingViewModel);
                    if (oldIndex != index)
                    {
                        ListItems.Move(oldIndex, index);
                    }
                }
                else
                {
                    // VM 不存在，创建
                    var newViewModel = new MusicVersionListItemViewModel(Model, CommandManager, this, data);
                    ListItems.Insert(index, newViewModel);
                }

                index++;
            }

            // 选中 VM 被删除时，取消选中
            if (selectedMusicVersionData.Value != null && !hashSet.Contains(selectedMusicVersionData.Value))
            {
                selectedMusicVersionData.Value = null;
            }
        }

        private void RefreshData(MusicVersionData data)
        {
            if (SelectedMusicVersionData.Value != data)
                return;

            detailTitle.Value = data.VersionTitle;
            detailAudioFilePath.Value = data.AudioFilePath;
            detailOffset.Value = data.Offset.ToString();
        }


        public void AddMusicVersionItem()
        {
            throw new NotImplementedException();
        }

        public void CloseCanvas()
        {
            Model.MusicVersionCanvasVisibility.Value = false;
        }

        public void SetTitle(string title)
        {
            if (selectedMusicVersionData.Value == null)
                throw new NullReferenceException("禁止在未选中版本时修改标题，请检查！");

            Model.UpdateMusicVersionItemBasicData(selectedMusicVersionData.Value, newVersionTitle: title);
        }

        public void ImportAudioFile()
        {
            GameRoot.File.OpenLoadFilePathBrowser((path) =>
            {
                return;
            });
            throw new NotImplementedException();
        }

        public void MinusOffset()
        {
            if (selectedMusicVersionData.Value == null)
                throw new NullReferenceException("禁止在未选中版本时修改 offset，请检查！");

            int newOffset = selectedMusicVersionData.Value.Offset - AddOffsetStep;

            Model.UpdateMusicVersionItemBasicData(selectedMusicVersionData.Value, newOffset: newOffset);
        }

        public void SetOffset(string offset)
        {
            if (selectedMusicVersionData.Value == null)
                throw new NullReferenceException("禁止在未选中版本时修改 offset，请检查！");

            if (!int.TryParse(offset, out int newOffset))
            {
                detailOffset.ForceNotify();
                return;
            }

            Model.UpdateMusicVersionItemBasicData(selectedMusicVersionData.Value, newOffset: newOffset);
        }

        public void AddOffset()
        {
            if (selectedMusicVersionData.Value == null)
                throw new NullReferenceException("禁止在未选中版本时修改 offset，请检查！");

            int newOffset = selectedMusicVersionData.Value.Offset + AddOffsetStep;

            Model.UpdateMusicVersionItemBasicData(selectedMusicVersionData.Value, newOffset: newOffset);
        }

        public void TestOffset()
        {
            throw new NotImplementedException();
        }

        public void AddStaffItem()
        {
            throw new NotImplementedException();
        }


        public void DeleteItem()
        {
        }

        public void CloneItem()
        {
            throw new NotImplementedException();
        }

        public void MoveUpItem()
        {
            throw new NotImplementedException();
        }

        public void MoveDownItem()
        {
            throw new NotImplementedException();
        }

        public void TopItem()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 由 ListItem 子 VM 调用，选择一个 item 并编辑
        /// </summary>
        public void SelectEditingMusicVersionData(MusicVersionData data)
        {
            selectedMusicVersionData.Value = data;
        }
    }
}
