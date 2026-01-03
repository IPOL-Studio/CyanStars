#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionViewModel : BaseViewModel
    {
        private const int AddOffsetStep = 10;


        public readonly ISynchronizedView<MusicVersionDataEditorModel, MusicVersionListItemViewModel> ListItems;

        private readonly ReactiveProperty<MusicVersionDataEditorModel?> selectedMusicVersionData;
        public ReadOnlyReactiveProperty<MusicVersionDataEditorModel?> SelectedMusicVersionData => selectedMusicVersionData;

        private readonly ObservableList<KeyValuePair<string, List<string>>> staffItemsProxy;
        public readonly ISynchronizedView<KeyValuePair<string, List<string>>, MusicVersionStaffItemViewModel> StaffItems;


        public readonly ReadOnlyReactiveProperty<bool> CanvasVisibility;
        public readonly ReadOnlyReactiveProperty<bool> ListVisibility;
        public readonly ReadOnlyReactiveProperty<bool> DetailVisibility;

        public readonly ReadOnlyReactiveProperty<string> DetailTitle;
        public readonly ReadOnlyReactiveProperty<string> DetailAudioFilePath;
        public readonly ReadOnlyReactiveProperty<string> DetailOffset;


        public MusicVersionViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            // 初始化 ListItems
            ListItems = Model.ChartPackData.CurrentValue.MusicVersions
                .CreateView(data => new MusicVersionListItemViewModel(model, commandManager, this, data))
                .AddTo(base.Disposables);

            // 如果音乐版本不为空，就选择首个音乐版本数据作为初始选中项
            selectedMusicVersionData = new ReactiveProperty<MusicVersionDataEditorModel?>(
                    Model.ChartPackData.CurrentValue.MusicVersions.Count >= 1
                        ? Model.ChartPackData.CurrentValue.MusicVersions[0]
                        : null
                )
                .AddTo(base.Disposables);

            // 初始化代理集合和视图
            staffItemsProxy = new ObservableList<KeyValuePair<string, List<string>>>();
            StaffItems = staffItemsProxy
                .CreateView(kvp => new MusicVersionStaffItemViewModel(model, commandManager, this, kvp))
                .AddTo(base.Disposables);

            // 设置 StaffItems 数据同步逻辑，并使用 SerialDisposable 来管理当前选中 Staffs 集合的事件订阅，切换选中项时自动取消上一次的订阅
            var staffSyncDisposable = new SerialDisposable().AddTo(base.Disposables);
            selectedMusicVersionData
                .Subscribe(data =>
                    {
                        // 当选择的音乐版本变化时，刷新整个 Staff 列表
                        staffItemsProxy.Clear();

                        if (data == null)
                        {
                            staffSyncDisposable.Disposable = null;
                            return;
                        }

                        foreach (var item in data.Staffs)
                        {
                            staffItemsProxy.Add(item);
                        }

                        var d = new CompositeDisposable();

                        data.Staffs.ObserveAdd().Subscribe(e => staffItemsProxy.Add(e.Value)).AddTo(d);
                        data.Staffs.ObserveRemove().Subscribe(e => staffItemsProxy.Remove(e.Value)).AddTo(d);
                        data.Staffs.ObserveReplace().Subscribe(e => staffItemsProxy[e.Index] = e.NewValue).AddTo(d);
                        data.Staffs.ObserveMove().Subscribe(e => staffItemsProxy.Move(e.OldIndex, e.NewIndex)).AddTo(d);
                        data.Staffs.ObserveReset().Subscribe(_ => staffItemsProxy.Clear()).AddTo(d);

                        staffSyncDisposable.Disposable = d;
                    }
                )
                .AddTo(base.Disposables);


            CanvasVisibility = Model.MusicVersionCanvasVisibility;
            ListVisibility = Observable
                .CombineLatest(
                    model.IsSimplificationMode,
                    model.ChartPackData
                        .Select(data => data.MusicVersions.ObserveCountChanged(notifyCurrentCount: true))
                        .Switch(),
                    (isSimplificationMode, count) =>
                        !(isSimplificationMode && count == 1)
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            Model.ChartPackData // 元素数量变化后自动选择一个版本
                .Select(data =>
                    data.MusicVersions.ObserveCountChanged(notifyCurrentCount: true)
                        .Select(_ => data.MusicVersions)
                )
                .Switch()
                .Subscribe(list =>
                    {
                        if (list.Count >= 1 && selectedMusicVersionData.Value == null)
                            selectedMusicVersionData.Value = list[0];
                    }
                )
                .AddTo(base.Disposables);
            DetailVisibility = selectedMusicVersionData
                .Select(data => data != null)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            DetailTitle = SelectedMusicVersionData
                .Select(data => data?.VersionTitle ?? Observable.Return("").AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(SelectedMusicVersionData.CurrentValue?.VersionTitle.Value ?? "")
                .AddTo(base.Disposables);
            DetailAudioFilePath = SelectedMusicVersionData
                .Select(data => data?.AudioFilePath ?? Observable.Return("").AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(SelectedMusicVersionData.CurrentValue?.AudioFilePath.Value ?? "")
                .AddTo(base.Disposables);
            DetailOffset = SelectedMusicVersionData
                .Select(data => data?.Offset ?? Observable.Return(0).AsObservable())
                .Switch()
                .Select(offset => offset.ToString())
                .ToReadOnlyReactiveProperty(SelectedMusicVersionData.CurrentValue?.Offset.Value.ToString() ?? "0")
                .AddTo(base.Disposables);
        }

        /// <summary>
        /// 由子 VM 调用，切换正在编辑的音乐版本数据
        /// </summary>
        /// <param name="musicVersionData">音乐版本数据</param>
        public void SelectEditingMusicVersionData(MusicVersionDataEditorModel? musicVersionData)
        {
            if (selectedMusicVersionData.CurrentValue == musicVersionData)
                return;

            var oldValue = selectedMusicVersionData.CurrentValue;
            CommandManager.ExecuteCommand(new DelegateCommand(
                () => selectedMusicVersionData.Value = musicVersionData,
                () => selectedMusicVersionData.Value = oldValue
            ));
        }

        /// <summary>
        /// 由子 VM 调用，删除并重建 Staff 数据，字典不保证排序一致，新的数据大概率会被添加到末尾
        /// </summary>
        public void RebuildStaffItemData(
            KeyValuePair<string, List<string>> oldData,
            KeyValuePair<string, List<string>> newData
        )
        {
            if (selectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下替换 Staff 数据。");

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        selectedMusicVersionData.CurrentValue.Staffs.Remove(oldData);
                        selectedMusicVersionData.CurrentValue.Staffs.Add(newData);
                    },
                    () =>
                    {
                        selectedMusicVersionData.CurrentValue.Staffs.Remove(newData);
                        selectedMusicVersionData.CurrentValue.Staffs.Add(oldData);
                    }
                )
            );
        }

        /// <summary>
        /// 由子 VM 调用，在选定的版本中删除一行 Staff 信息
        /// </summary>
        public void DeleteStaffItemData(KeyValuePair<string, List<string>> data)
        {
            if (selectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下修改 Staff 数据。");

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        selectedMusicVersionData.CurrentValue.Staffs.Remove(data);
                    },
                    () =>
                    {
                        selectedMusicVersionData.CurrentValue.Staffs.Add(data);
                    }
                )
            );
        }

        /// <summary>
        /// 由子 VM 调用，检查新的 StaffName 可用性（不能与已有的重复）
        /// </summary>
        public bool CheckNewStaffNameAvailable(string newStaffName)
        {
            if (selectedMusicVersionData.CurrentValue == null)
                return false;

            foreach (var staffKvp in selectedMusicVersionData.CurrentValue.Staffs)
            {
                if (staffKvp.Key == newStaffName)
                    return false;
            }

            return true;
        }


        public void AddMusicVersionItem()
        {
            var newMusicVersionData = new MusicVersionDataEditorModel(new MusicVersionData("新音乐版本"));
            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.MusicVersions.Add(newMusicVersionData);
                    },
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.MusicVersions.Remove(newMusicVersionData);
                    }
                )
            );
        }

        public void AddStaffItem()
        {
            if (selectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下添加 Staff 数据。");

            int i = 1;
            while (true)
            {
                bool isRepeated = false;
                foreach (var staffData in selectedMusicVersionData.CurrentValue.Staffs)
                {
                    if (staffData.Key == $"Staff{i}")
                    {
                        isRepeated = true;
                        break;
                    }
                }

                if (isRepeated)
                    i++;
                else
                    break;
            }

            selectedMusicVersionData.CurrentValue.Staffs.Add(
                new KeyValuePair<string, List<string>>($"Staff{i}", new List<string>())
            );
        }


        public void CloseCanvas()
        {
            if (!CanvasVisibility.CurrentValue)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(
                () => Model.MusicVersionCanvasVisibility.Value = false,
                () => Model.MusicVersionCanvasVisibility.Value = true
            ));
        }

        public void SetTitle(string newTitle)
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置标题。");

            var oldTitle = SelectedMusicVersionData.CurrentValue!.VersionTitle.Value;
            if (oldTitle == newTitle)
                return;
            CommandManager.ExecuteCommand(new DelegateCommand(
                () => SelectedMusicVersionData.CurrentValue!.VersionTitle.Value = newTitle,
                () => SelectedMusicVersionData.CurrentValue!.VersionTitle.Value = oldTitle
            ));
        }

        public void ImportAudioFile()
        {
            throw new NotImplementedException();
        }

        public void MinusOffset()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            CommandManager.ExecuteCommand(new DelegateCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value -= AddOffsetStep,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value += AddOffsetStep
            ));
        }

        public void SetOffset(string text)
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            if (!int.TryParse(text, out int newValue))
            {
                SelectedMusicVersionData.CurrentValue?.Offset.ForceNotify();
                return;
            }

            int oldValue = SelectedMusicVersionData.CurrentValue!.Offset.Value;
            if (oldValue == newValue)
                return;
            CommandManager.ExecuteCommand(new DelegateCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value = newValue,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value = oldValue
            ));
        }

        public void AddOffset()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            CommandManager.ExecuteCommand(new DelegateCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value += AddOffsetStep,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value -= AddOffsetStep
            ));
        }

        public void TestOffset()
        {
            throw new NotImplementedException();
        }

        public void DeleteItem()
        {
            throw new NotImplementedException();
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
    }
}
