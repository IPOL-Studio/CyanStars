#nullable enable

using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Chart.Loading;
using CyanStars.Framework;
using CyanStars.Framework.File;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionViewModel : BaseViewModel
    {
        private const int AddOffsetStep = 10;


        /// <summary>
        /// 是否有音乐正在加载：加载完成前拒绝新的加载请求
        /// </summary>
        private bool isAudioLoading;

        /// <summary>
        /// 本 ViewModel 是否已经结束生命周期，用于丢弃退出后的异步加载结果
        /// </summary>
        private bool isDisposed;


        public ReadOnlyReactiveProperty<bool> IsMultiMusicItemMode => Model.IsMultiMusicItemMode;
        public ReadOnlyReactiveProperty<ChartPackDataEditorModel> ChartPackData => Model.ChartPackData;

        public readonly ISynchronizedView<MusicVersionDataEditorModel, MusicVersionListItemViewModel> MusicListItems;

        private readonly ReactiveProperty<MusicVersionDataEditorModel?> selectedMusicVersionData;
        public ReadOnlyReactiveProperty<MusicVersionDataEditorModel?> SelectedMusicVersionData => selectedMusicVersionData;

        public readonly ReadOnlyReactiveProperty<string> DetailTitle;
        public readonly ReadOnlyReactiveProperty<string> DetailAudioFilePath;
        public readonly ReadOnlyReactiveProperty<string> DetailOffset;


        public MusicVersionViewModel(ChartEditorModel model)
            : base(model)
        {
            // 如果音乐版本不为空，就选择首个音乐版本数据作为初始选中项
            selectedMusicVersionData = new ReactiveProperty<MusicVersionDataEditorModel?>(
                    Model.ChartPackData.CurrentValue.MusicVersions.Count >= 1
                        ? Model.ChartPackData.CurrentValue.MusicVersions[0]
                        : null
                )
                .AddTo(base.Disposables);

            // 初始化 MusicListItems
            MusicListItems = Model.ChartPackData.CurrentValue.MusicVersions
                .CreateView(data => new MusicVersionListItemViewModel(model, this, data))
                .AddTo(base.Disposables);

            Model.ChartPackData // 元素数量变化时更新选中的元素
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
                    }
                )
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
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, SelectedMusicVersionData.CurrentValue?.Offset.Value.ToString() ?? "0")
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
            // 纯选中状态变化，不影响持久化数据，不参与脏判定
            CommandStack.ExecuteCommand(
                () => selectedMusicVersionData.Value = musicVersionData,
                () => selectedMusicVersionData.Value = oldValue,
                affectsSavedData: false
            );
        }

        public void AddMusicVersionItem()
        {
            var newMusicVersionData = new MusicVersionDataEditorModel(new MusicVersionData("新音乐版本"));
            CommandStack.ExecuteCommand(
                () => Model.ChartPackData.CurrentValue.MusicVersions.Add(newMusicVersionData),
                () => Model.ChartPackData.CurrentValue.MusicVersions.Remove(newMusicVersionData)
            );
        }

        /// <summary>
        /// 在关闭 Canvas 时调用，加载首个音乐版本的 Audio
        /// </summary>
        public void LoadAudio()
        {
            // 当前采用拒绝并发的降级方案：在途加载完成前忽略新的加载请求
            // TODO: 改用 cts 支持取消和打断在途加载
            if (isAudioLoading)
            {
                Debug.LogWarning("上一份音乐尚未加载完成，已忽略本次加载请求。");
                return;
            }

            // 停止正在播放的音乐
            if (Model.IsTimelinePlaying.CurrentValue)
                Model.IsTimelinePlaying.Value = false;

            // 关闭弹窗时，卸载原有的音乐并尝试加载首个元素作为制谱器内播放的音乐。这个操作无需撤销。
            // TODO: 优化加载逻辑，只在音频文件真的不同时加载
            if (Model.ChartPackData.CurrentValue.MusicVersions.Count > 0 &&
                !string.IsNullOrEmpty(Model.ChartPackData.CurrentValue.MusicVersions[0].AudioFilePath.CurrentValue))
            {
                string musicFilePath = Model.ResolveAssetReadablePath(Model.ChartPackData.CurrentValue.MusicVersions[0].AudioFilePath.CurrentValue);

                LoadAudio(musicFilePath);
                Debug.Log($"已加载音乐：{musicFilePath}");
            }
            else
            {
                LoadAudio(null);
                Debug.Log("已卸载音乐");
            }
        }

        /// <summary>
        /// 停止播放音乐，卸载旧音乐并尝试加载新音乐
        /// </summary>
        /// <param name="audioFilePath">新音乐文件的绝对路径，路径为 null 将仅卸载原有的音乐并卸载 handler，路径无效将产生一个包含 null 的 handler 实例</param>
        private async void LoadAudio(string? audioFilePath)
        {
            isAudioLoading = true;
            try
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

                    if (isDisposed)
                    {
                        // 加载期间退出了制谱器，直接卸载，避免句柄泄漏
                        handler.Unload();
                        return;
                    }

                    Model.AudioClipHandler.Value = handler;
                    if (handler.Asset == null)
                        Debug.LogWarning("加载音乐失败！");
                }
            }
            finally
            {
                isAudioLoading = false;
            }
        }

        public void SetTitle(string newTitle)
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置标题。");

            var oldTitle = SelectedMusicVersionData.CurrentValue!.VersionTitle.Value;
            if (oldTitle == newTitle)
                return;
            CommandStack.ExecuteCommand(
                () => SelectedMusicVersionData.CurrentValue!.VersionTitle.Value = newTitle,
                () => SelectedMusicVersionData.CurrentValue!.VersionTitle.Value = oldTitle
            );
        }

        public void ImportAudioFile()
        {
            // 打开对话框时先锁定导入目标版本：对话框是异步的，期间玩家可能切换选中项，撤销会作用到别的版本上
            MusicVersionDataEditorModel? targetMusicVersion = selectedMusicVersionData.CurrentValue;
            if (targetMusicVersion == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下导入音乐。");

            GameRoot.File.OpenLoadFileHandleBrowser(
                audioFileHandle => ImportMusicFile(targetMusicVersion, audioFileHandle),
                title: "选择音乐文件",
                filters: new[]
                {
                    ChartEditorFileFilters.Audio
                },
                cacheScope: ChartEditorModel.AssetCacheScope
            );
        }

        /// <summary>
        /// 把外部音频文件导入到指定音乐版本
        /// </summary>
        /// <param name="targetMusicVersion">导入到哪个版本</param>
        /// <param name="audioFileHandle">选中音频文件的句柄，成功导入后句柄的生命周期转交给 Model</param>
        private void ImportMusicFile(MusicVersionDataEditorModel targetMusicVersion, FileHandle audioFileHandle)
        {
            // 对话框期间该版本可能已经被删掉了
            // FileBrowser 通常会实时检测文件存在状态并从对话框中删除文件避免选中，此处仅作为防御措施
            if (!Model.ChartPackData.CurrentValue.MusicVersions.Contains(targetMusicVersion))
            {
                GameRoot.File.Portal.TryReleaseFile(audioFileHandle);

                PopupView.Show("无法导入音乐",
                    "目标音乐版本已被删除，请重新选择要导入的版本。",
                    true,
                    new Dictionary<string, Action?>
                    {
                        ["确定"] = null
                    }
                );
                return;
            }

            // 校验文件名是否和其他版本重复
            string newTargetRelativePath = PathUtil.Combine(
                ChartPackDataLoader.ChartPackAssetsFolder,
                audioFileHandle.EntryName
            );

            foreach (var musicVersionData in Model.ChartPackData.CurrentValue.MusicVersions)
            {
                if (ReferenceEquals(musicVersionData, targetMusicVersion))
                    continue;

                // 按路径语义比较：同名文件的路径写法可能不同，但在磁盘上是同一个文件
                if (!PathUtil.PathComparer.Equals(
                        musicVersionData.AudioFilePath.CurrentValue,
                        newTargetRelativePath
                    ))
                    continue;

                GameRoot.File.Portal.TryReleaseFile(audioFileHandle);

                PopupView.Show("无法导入音乐",
                    "选中的音乐文件文件名与其他音乐版本文件名重复，请重命名后再次导入",
                    true,
                    new Dictionary<string, Action?>
                    {
                        ["确定"] = null
                    }
                );
                return;
            }

            var oldTargetRelativePath = targetMusicVersion.AudioFilePath.CurrentValue;
            if (oldTargetRelativePath == null)
                oldTargetRelativePath = "";
            var oldTargetAbsolutePath = Model.GetAssetAbsolutePath(oldTargetRelativePath);

            var newTargetAbsolutePath = Model.GetAssetAbsolutePath(newTargetRelativePath);

            // 提前校验保存目标，失败则不修改任何数据
            if (!GameRoot.File.Portal.TrySetSaveTarget(audioFileHandle, newTargetAbsolutePath))
            {
                GameRoot.File.Portal.TryReleaseFile(audioFileHandle);

                PopupView.Show("无法导入音乐",
                    "无法把选中的音乐保存到谱包工作区，请检查日志。",
                    true,
                    new Dictionary<string, Action?>
                    {
                        ["确定"] = null
                    }
                );
                return;
            }

            // 捕获旧句柄：同名导入顶替自身或撤销时，要用它把旧内容换回来
            FileHandle? oldAssetHandle = Model.FindAssetHandle(oldTargetAbsolutePath);

            CommandStack.ExecuteCommand(() =>
                {
                    targetMusicVersion.AudioFilePath.Value = newTargetRelativePath;

                    Model.AddAssetHandle(newTargetAbsolutePath, audioFileHandle);

                    // 旧音频若已经不被任何音乐版本引用，就解绑它的句柄；句柄本身保留到会话结束，撤销时还要用
                    if (!Model.IsAssetReferenced(oldTargetAbsolutePath))
                        Model.DetachAssetHandle(oldTargetAbsolutePath);
                },
                () =>
                {
                    targetMusicVersion.AudioFilePath.Value = oldTargetRelativePath;

                    Model.DetachAssetHandle(newTargetAbsolutePath);

                    if (oldAssetHandle != null)
                        Model.ReattachAssetHandle(oldTargetAbsolutePath, oldAssetHandle);
                }
            );
        }

        public void MinusOffset()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            CommandStack.ExecuteCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value -= AddOffsetStep,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value += AddOffsetStep
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
            CommandStack.ExecuteCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value = newValue,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value = oldValue
            );
        }

        public void AddOffset()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            CommandStack.ExecuteCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value += AddOffsetStep,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value -= AddOffsetStep
            );
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

            // 该版本的音频可能来自本次会话的导入，删除时要一并解除它对句柄的占用
            string audioAbsolutePath = GetAbsoluteAudioFilePath(oldData);
            FileHandle? audioAssetHandle = Model.FindAssetHandle(audioAbsolutePath);

            CommandStack.ExecuteCommand(
                () =>
                {
                    Model.ChartPackData.CurrentValue.MusicVersions.RemoveAt(selectedIndex);

                    if (!string.IsNullOrEmpty(audioAbsolutePath) && !Model.IsAssetReferenced(audioAbsolutePath))
                        Model.DetachAssetHandle(audioAbsolutePath);

                    selectedMusicVersionData.Value = null;
                },
                () =>
                {
                    Model.ChartPackData.CurrentValue.MusicVersions.Insert(selectedIndex, oldData);

                    // 句柄在会话结束前都还留着缓存副本，撤销时可以取回来
                    if (audioAssetHandle != null)
                        Model.ReattachAssetHandle(audioAbsolutePath, audioAssetHandle);

                    selectedMusicVersionData.Value = Model.ChartPackData.CurrentValue.MusicVersions[selectedIndex];
                }
            );
        }

        /// <summary>
        /// 取某个音乐版本的音频在工作区里的绝对路径
        /// </summary>
        /// <returns>没有指定音频时返回空字符串</returns>
        private string GetAbsoluteAudioFilePath(MusicVersionDataEditorModel musicVersionData)
        {
            return Model.GetAssetAbsolutePath(musicVersionData.AudioFilePath.CurrentValue);
        }

        public void CloneItem()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下克隆版本。");

            int clonedItemIndex = Model.ChartPackData.CurrentValue.MusicVersions.Count;

            CommandStack.ExecuteCommand(
                () =>
                {
                    var deepClonedData = new MusicVersionDataEditorModel(
                        new MusicVersionData(
                            SelectedMusicVersionData.CurrentValue.VersionTitle.Value,
                            SelectedMusicVersionData.CurrentValue.AudioFilePath.Value,
                            SelectedMusicVersionData.CurrentValue.Offset.Value
                        )
                    );

                    Model.ChartPackData.CurrentValue.MusicVersions.Add(deepClonedData);
                },
                () =>
                {
                    Model.ChartPackData.CurrentValue.MusicVersions.RemoveAt(clonedItemIndex);
                }
            );
        }

        public void MoveUpItem()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下移动版本。");

            int oldIndex = Model.ChartPackData.CurrentValue.MusicVersions.IndexOf(SelectedMusicVersionData.CurrentValue);
            if (oldIndex == 0)
                return; // 首个元素不能上移

            CommandStack.ExecuteCommand(
                () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex, oldIndex - 1),
                () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex - 1, oldIndex)
            );
        }

        public void MoveDownItem()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下移动版本。");

            int oldIndex = Model.ChartPackData.CurrentValue.MusicVersions.IndexOf(SelectedMusicVersionData.CurrentValue);
            if (oldIndex == Model.ChartPackData.CurrentValue.MusicVersions.Count - 1)
                return; // 末个元素不能下移

            CommandStack.ExecuteCommand(
                () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex, oldIndex + 1),
                () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex + 1, oldIndex)
            );
        }

        public void TopItem()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下移动版本。");

            int oldIndex = Model.ChartPackData.CurrentValue.MusicVersions.IndexOf(SelectedMusicVersionData.CurrentValue);
            if (oldIndex == 0)
                return; // 首个元素不能置顶

            CommandStack.ExecuteCommand(
                () => Model.ChartPackData.CurrentValue.MusicVersions.Move(oldIndex, 0),
                () => Model.ChartPackData.CurrentValue.MusicVersions.Move(0, oldIndex)
            );
        }

        public override void Dispose()
        {
            isDisposed = true;
            base.Dispose();
        }
    }
}
