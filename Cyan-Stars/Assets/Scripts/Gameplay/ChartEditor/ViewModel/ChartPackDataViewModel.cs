#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.File;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Management;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Utils;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartPackDataViewModel : BaseViewModel
    {
        private Vector2? dragStartCropPos;
        private float? dragStartCropHeight;

        public ReadOnlyReactiveProperty<ChartPackDataEditorModel> ChartPackData => Model.ChartPackData;

        public readonly ReadOnlyReactiveProperty<string> ChartPackTitle;
        public readonly ReadOnlyReactiveProperty<Beat> PreviewStartBeat;
        public readonly ReadOnlyReactiveProperty<Beat> PreviewEndBeat;
        public readonly ReadOnlyReactiveProperty<string> CoverFilePathString;
        public readonly ReadOnlyReactiveProperty<string> ChartPackInfo;


        public ChartPackDataViewModel(ChartEditorModel model)
            : base(model)
        {
            ChartPackTitle = Model.ChartPackData
                .Select(data => data.Title.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.Title.Value)
                .AddTo(base.Disposables);

            PreviewStartBeat = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<Beat>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.CurrentValue
                )
                .AddTo(base.Disposables);
            PreviewEndBeat = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<Beat>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.CurrentValue
                )
                .AddTo(base.Disposables);

            CoverFilePathString = Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .Select(path => path ?? "")
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);

            ChartPackInfo = Model.ChartPackData
                .Select(data => data.ChartPackInfo.AsObservable())
                .Switch()
                .Select(info => info ?? "")
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);
        }


        public void SetChartPackTitle(string newTitle)
        {
            string oldTitle = Model.ChartPackData.CurrentValue.Title.Value;
            if (newTitle == oldTitle)
                return;

            CommandStack.ExecuteCommand(
                () => Model.ChartPackData.CurrentValue.Title.Value = newTitle,
                () => Model.ChartPackData.CurrentValue.Title.Value = oldTitle
            );
        }

        public void SetPreviewStartBeat(Beat newBeat)
        {
            if (newBeat > Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value)
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value;

            if (newBeat == oldBeat)
                return;

            CommandStack.ExecuteCommand(
                () => Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value = newBeat,
                () => Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value = oldBeat
            );
        }

        public void SetPreviewEndBeat(Beat newBeat)
        {
            if (newBeat < Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value)
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value;

            if (newBeat == oldBeat)
                return;

            CommandStack.ExecuteCommand(
                () => Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value = newBeat,
                () => Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value = oldBeat
            );
        }

        public void UpdateInfo(string newText)
        {
            // TODO: 实时更新字段 + 一段时间停止输入或失焦时压入 CommandStack
            var oldText = Model.ChartPackData.CurrentValue.ChartPackInfo.CurrentValue;
            if (newText == oldText)
                return;

            CommandStack.ExecuteCommand(
                () => Model.ChartPackData.CurrentValue.ChartPackInfo.Value = newText,
                () => Model.ChartPackData.CurrentValue.ChartPackInfo.Value = oldText
            );
        }

        public void ExportChartPack()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                PopupView.Show("无法导出谱包",
                    "暂不支持在安卓平台导出谱包。",
                    true,
                    new Dictionary<string, Action?>
                    {
                        ["确定"] = null
                    }
                );
                return;
            }

            // TODO: 将导出的文件打包为一个专有后缀名的文件
            GameRoot.File.OpenSaveFolderPathBrowser(targetParentPath =>
                {
                    // 1. 先在内存中固定和校验导出的目标数据
                    string folderName = PathUtil.GetName(Model.WorkspacePath);
                    string destPath = PathUtil.Combine(targetParentPath, folderName);

                    // 防止导出到工作区内部时递归复制自我嵌套
                    if (PathUtil.IsSubPathOf(destPath, Model.WorkspacePath))
                    {
                        PopupView.Show("无法导出谱包",
                            "不能把谱包导出到它自己的工作区内部，请选一个其他路径",
                            true,
                            new Dictionary<string, Action?>
                            {
                                ["确定"] = null
                            }
                        );
                        return;
                    }

                    // 防止有人把谱包导出到应用数据路径（尤其是要导出的谱包内）下，然后用无限递归炸掉程序（以及磁盘空间和资源管理器）
                    Uri parentUri = new Uri(Path.GetFullPath(Application.persistentDataPath));
                    Uri targetUri = new Uri(Path.GetFullPath(targetParentPath));
                    if (parentUri.IsBaseOf(targetUri))
                    {
                        PopupView.Show("无法导出谱包",
                            "不能导出到游戏数据目录内部，请选一个其他路径",
                            true,
                            new Dictionary<string, Action?>
                            {
                                ["确定"] = null
                            }
                        );
                        return;
                    }

                    // 2. 再把当前谱包保存到玩家数据路径，保存失败则取消导出
                    if (!ChartEditorSaver.SaveChartAndAssetsToDisk(Model))
                    {
                        PopupView.Show("无法导出谱包",
                            "保存谱包数据失败，已取消导出。具体原因见日志。",
                            true,
                            new Dictionary<string, Action?>
                            {
                                ["确定"] = null
                            }
                        );
                        return;
                    }

                    // 3. 最后把工作区复制到指定路径，已存在同名文件夹时依次添加 "(1)" 等后缀
                    if (!FolderUtil.TryCopyFolder(Model.WorkspacePath, destPath, out _))
                    {
                        PopupView.Show("无法导出谱包",
                            "复制谱包文件失败，具体原因见日志。",
                            true,
                            new Dictionary<string, Action?>
                            {
                                ["确定"] = null
                            }
                        );
                    }
                },
                null,
                "导出到"
            );
        }
    }
}
