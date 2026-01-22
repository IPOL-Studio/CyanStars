#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.File;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionViewModel : BaseViewModel
    {
        private const int AddOffsetStep = 10;

        public readonly ISynchronizedView<MusicVersionDataEditorModel, MusicVersionListItemViewModel> MusicListItems;

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
            // 初始化 MusicListItems
            MusicListItems = Model.ChartPackData.CurrentValue.MusicVersions
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
                    SelectedMusicVersionData,
                    (isSimplificationMode, count, selectedData) =>
                        !(isSimplificationMode && count == 1 && selectedData != null)
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            Model.ChartPackData // 元素数量更新选中的元素
                .Select(data =>
                    data.MusicVersions.ObserveCountChanged(notifyCurrentCount: true).Select(_ => data.MusicVersions)
                )
                .Switch()
                .Subscribe(_ =>
                    {
                        // 如果选中的元素被删除了，将选中元素设为 null
                        if (selectedMusicVersionData.CurrentValue != null &&
                            !Model.ChartPackData.CurrentValue.MusicVersions.Contains(selectedMusicVersionData.CurrentValue))
                            selectedMusicVersionData.Value = null;

                        // 如果处于简易模式，自动选中首个元素
                        if (Model.ChartPackData.CurrentValue.MusicVersions.Count > 0 &&
                            Model.IsSimplificationMode.CurrentValue)
                            selectedMusicVersionData.Value = Model.ChartPackData.CurrentValue.MusicVersions[0];
                    }
                )
                .AddTo(base.Disposables);
            DetailVisibility =
                selectedMusicVersionData
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
            CommandManager.ExecuteCommand(
                new DelegateCommand(
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
                    () => selectedMusicVersionData.CurrentValue.Staffs.Remove(data),
                    () => selectedMusicVersionData.CurrentValue.Staffs.Add(data)
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
                    () => Model.ChartPackData.CurrentValue.MusicVersions.Add(newMusicVersionData),
                    () => Model.ChartPackData.CurrentValue.MusicVersions.Remove(newMusicVersionData)
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

            // 关闭弹窗时，卸载原有的音乐并尝试加载首个元素作为制谱器内播放的音乐。这个操作无需撤销。
            // TODO: 优化加载逻辑，只在音频文件真的不同时加载
            if (Model.ChartPackData.CurrentValue.MusicVersions.Count > 0 &&
                !string.IsNullOrEmpty(Model.ChartPackData.CurrentValue.MusicVersions[0].AudioFilePath.CurrentValue))
            {
                // 如果能根据 targetPath 找到暂存文件句柄，则优先加载句柄指向的缓存文件，否则加载谱包资源文件夹内的文件。
                string musicFilePath = PathUtil.Combine(Model.WorkspacePath, Model.ChartPackData.CurrentValue.MusicVersions[0].AudioFilePath.CurrentValue);
                var handler = ChartEditorFileManager.GetHandlerByTargetPath(musicFilePath);
                if (handler != null)
                {
                    musicFilePath = handler.TempFilePath;
                }

                LoadAudio(musicFilePath);
                Debug.Log($"已加载音乐：{musicFilePath}");
            }
            else
            {
                LoadAudio(null);
                Debug.Log("已卸载音乐");
            }

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => Model.MusicVersionCanvasVisibility.Value = false,
                    () => Model.MusicVersionCanvasVisibility.Value = true
                ));
        }

        /// <summary>
        /// 停止播放音乐，卸载旧音乐并尝试加载新音乐
        /// </summary>
        /// <param name="audioFilePath">新音乐文件的绝对路径，路径为 null 将仅卸载原有的音乐并卸载 handler，路径无效将产生一个包含 null 的 handler 实例</param>
        private async void LoadAudio(string? audioFilePath)
        {
            // TODO: 只在真的实际发生了变化时重新加载
            Model.IsTimelinePlaying.Value = false;

            if (Model.AudioClipHandler.CurrentValue != null)
            {
                Model.AudioClipHandler.CurrentValue.Unload();
                Model.AudioClipHandler.Value = null;
            }

            if (audioFilePath != null)
            {
                AssetHandler<AudioClip?>? handler = await GameRoot.Asset.LoadAssetAsync<AudioClip?>(audioFilePath);
                Model.AudioClipHandler.Value = handler;
                if (handler.Asset == null)
                    Debug.LogWarning("加载音乐失败！");
            }
        }

        public void SetTitle(string newTitle)
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置标题。");

            var oldTitle = SelectedMusicVersionData.CurrentValue!.VersionTitle.Value;
            if (oldTitle == newTitle)
                return;
            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => SelectedMusicVersionData.CurrentValue!.VersionTitle.Value = newTitle,
                    () => SelectedMusicVersionData.CurrentValue!.VersionTitle.Value = oldTitle
                )
            );
        }

        public void ImportAudioFile()
        {
            GameRoot.File.OpenLoadFilePathBrowser(
                ImportMusicFile,
                title: "选择音乐文件",
                filters: new[] { GameRoot.File.AudioFilter });
        }

        private void ImportMusicFile(string newOriginFilePath)
        {
            // throw new NotImplementedException();

            if (selectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置音频文件路径。");

            // 1. 校验要导入的音频文件名是否与其他版本文件名重复
            // 2. 如有旧缓存文件，记录其指向的目标地址以便撤销，然后将其置空
            // 3. 导入并暂存新音乐，将其指向新地址
            // 4. 记录并修改音乐版本数据内引用的地址

            var newTargetRelativePath = PathUtil.Combine(ChartEditorFileManager.ChartPackAssetsFolderName, Path.GetFileName(newOriginFilePath));

            foreach (var musicVersionData in Model.ChartPackData.CurrentValue.MusicVersions)
            {
                if (musicVersionData == selectedMusicVersionData.CurrentValue)
                    continue;

                if (musicVersionData.AudioFilePath.CurrentValue == newTargetRelativePath)
                {
                    Model.ShowPopup("无法导入音乐",
                        "选中的音乐文件文件名与其他音乐版本文件名重复，请重命名后再次导入",
                        new Dictionary<string, Action?> { ["确定"] = null },
                        true);
                    return;
                }
            }

            var oldTargetRelativePath = selectedMusicVersionData.CurrentValue.AudioFilePath.CurrentValue;
            if (string.IsNullOrEmpty(oldTargetRelativePath))
                oldTargetRelativePath = "";

            var oldTargetAbsolutePath = oldTargetRelativePath != "" ? PathUtil.Combine(Model.WorkspacePath, oldTargetRelativePath) : "";
            var newTargetAbsolutePath = PathUtil.Combine(Model.WorkspacePath, newTargetRelativePath);

            IReadonlyTempFileHandler? oldHandler = ChartEditorFileManager.GetHandlerByTargetPath(oldTargetAbsolutePath);

            // 仅复制文件到缓存区，暂不声明目标路径以防止自动顶替旧句柄目标路径。
            IReadonlyTempFileHandler newHandler = ChartEditorFileManager.TempFile(newOriginFilePath, null);

            CommandManager.ExecuteCommand(
                new DelegateCommand(() =>
                    {
                        selectedMusicVersionData.CurrentValue.AudioFilePath.Value = newTargetRelativePath;

                        if (oldHandler != null)
                        {
                            ChartEditorFileManager.UpdateTargetFilePath(oldHandler as TempFileHandler, null);
                        }

                        ChartEditorFileManager.UpdateTargetFilePath(newHandler as TempFileHandler, newTargetAbsolutePath);
                    },
                    () =>
                    {
                        selectedMusicVersionData.CurrentValue.AudioFilePath.Value = oldTargetRelativePath;

                        ChartEditorFileManager.UpdateTargetFilePath(newHandler as TempFileHandler, null);

                        if (oldHandler != null)
                        {
                            ChartEditorFileManager.UpdateTargetFilePath(oldHandler as TempFileHandler, oldTargetAbsolutePath);
                        }
                    }
                )
            );
        }

        public void MinusOffset()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => SelectedMusicVersionData.CurrentValue!.Offset.Value -= AddOffsetStep,
                    () => SelectedMusicVersionData.CurrentValue!.Offset.Value += AddOffsetStep
                )
            );
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
            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => SelectedMusicVersionData.CurrentValue!.Offset.Value = newValue,
                    () => SelectedMusicVersionData.CurrentValue!.Offset.Value = oldValue
                ));
        }

        public void AddOffset()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            CommandManager.ExecuteCommand(
                new DelegateCommand(
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
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下删除版本。");

            var oldData = SelectedMusicVersionData.CurrentValue;
            int selectedIndex = Model.ChartPackData.CurrentValue.MusicVersions.IndexOf(oldData);
            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.MusicVersions.RemoveAt(selectedIndex);
                        selectedMusicVersionData.Value = null;
                    },
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.MusicVersions.Insert(selectedIndex, oldData);
                        selectedMusicVersionData.Value = Model.ChartPackData.CurrentValue.MusicVersions[selectedIndex];
                    }
                )
            );
        }

        public void CloneItem()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下克隆版本。");

            int clonedItemIndex = Model.ChartPackData.CurrentValue.MusicVersions.Count;

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        // 深拷贝一个音乐版本数据
                        var staffs = new Dictionary<string, List<string>>();
                        foreach (var kvp in SelectedMusicVersionData.CurrentValue.Staffs)
                        {
                            var jobs = new List<string>();
                            foreach (var job in kvp.Value)
                            {
                                jobs.Add(job);
                            }

                            staffs.Add(kvp.Key, jobs);
                        }

                        var deepClonedData = new MusicVersionDataEditorModel(
                            new MusicVersionData(
                                SelectedMusicVersionData.CurrentValue.VersionTitle.Value,
                                SelectedMusicVersionData.CurrentValue.AudioFilePath.Value,
                                SelectedMusicVersionData.CurrentValue.Offset.Value,
                                staffs
                            )
                        );

                        Model.ChartPackData.CurrentValue.MusicVersions.Add(deepClonedData);
                    },
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.MusicVersions.RemoveAt(clonedItemIndex);
                    }
                )
            );
        }

        public void MoveUpItem()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下移动版本。");

            int oldIndex = Model.ChartPackData.CurrentValue.MusicVersions.IndexOf(SelectedMusicVersionData.CurrentValue);
            if (oldIndex == 0)
                return; // 首个元素不能上移

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex, oldIndex - 1),
                    () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex - 1, oldIndex)
                )
            );
        }

        public void MoveDownItem()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下移动版本。");

            int oldIndex = Model.ChartPackData.CurrentValue.MusicVersions.IndexOf(SelectedMusicVersionData.CurrentValue);
            if (oldIndex == Model.ChartPackData.CurrentValue.MusicVersions.Count - 1)
                return; // 末个元素不能下移

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex, oldIndex + 1),
                    () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex + 1, oldIndex)
                )
            );
        }

        public void TopItem()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下移动版本。");

            int oldIndex = Model.ChartPackData.CurrentValue.MusicVersions.IndexOf(SelectedMusicVersionData.CurrentValue);
            if (oldIndex == 0)
                return; // 首个元素不能置顶

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex, 0),
                    () => Model.ChartPackData.CurrentValue.MusicVersions.Move(0, oldIndex)
                )
            );
        }
    }
}
