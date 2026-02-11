#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Utils;
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

        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField1String;
        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField2String;
        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField3String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField1String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField2String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField3String;

        public readonly ReadOnlyReactiveProperty<string> CoverFilePathString;


        private const int MaxRecursiveDeep = 5;


        public ChartPackDataViewModel(ChartEditorModel model)
            : base(model)
        {
            ChartPackTitle = Model.ChartPackData
                .Select(data => data.Title.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.Title.Value)
                .AddTo(base.Disposables);

            PreviewStartBeatField1String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.IntegerPart.ToString()
                )
                .AddTo(base.Disposables);
            PreviewStartBeatField2String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.Numerator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewStartBeatField3String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.Denominator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField1String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.IntegerPart.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField2String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.Numerator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField3String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.Denominator.ToString()
                )
                .AddTo(base.Disposables);

            CoverFilePathString = Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .Select(path => path ?? "")
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

        public void SetPreviewStartBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat newBeat))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

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

        public void SetPreviewEndBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat newBeat))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

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

        public void ExportChartPack()
        {
            // TODO: 将导出的文件打包为一个专有后缀名的文件
            GameRoot.File.OpenSaveFolderPathBrowser(targetParentPath =>
                {
                    // 1. 先保存谱包谱面到玩家数据路径
                    ChartEditorFileManager.SaveChartAndAssetsToDesk(Model.WorkspacePath, Model.ChartMetaDataIndex, Model.ChartPackData.CurrentValue, Model.ChartData.CurrentValue);

                    // 2. 再将玩家数据路径的谱包文件夹复制到指定路径，如果已经存在同名文件夹，则添加 "(1)" 等后缀
                    DirectoryInfo sourceDirInfo = new DirectoryInfo(Model.WorkspacePath);
                    string folderName = sourceDirInfo.Name;
                    string destPath = PathUtil.Combine(targetParentPath, folderName);

                    // 防止有人把谱包导出到应用数据路径（尤其是要导出的谱包内）下，然后用无限递归炸掉程序（以及磁盘空间和资源管理器）
                    Uri parentUri = new Uri(Path.GetFullPath(Application.persistentDataPath));
                    Uri targetUri = new Uri(Path.GetFullPath(targetParentPath));
                    if (parentUri.IsBaseOf(targetUri))
                    {
                        PopupView.Show("无法导出谱包",
                            "不能导出到游戏数据目录内部，请选一个其他路径",
                            true,
                            new Dictionary<string, Action?> { ["确定"] = null }
                        );
                        return;
                    }

                    // 如果目标路径已存在，则循环尝试添加 (n) 后缀
                    if (Directory.Exists(destPath))
                    {
                        int counter = 1;
                        string baseDestPath = destPath;

                        while (Directory.Exists(destPath))
                        {
                            destPath = $"{baseDestPath} ({counter})";
                            counter++;
                        }
                    }

                    // 创建最终确定的目标文件夹，并递归复制内容
                    Directory.CreateDirectory(destPath);
                    RecursiveCopy(sourceDirInfo, new DirectoryInfo(destPath), 0);
                },
                null,
                "导出到"
            );
        }

        /// <summary>
        /// 递归复制文件和子目录的辅助方法
        /// </summary>
        private static void RecursiveCopy(DirectoryInfo source, DirectoryInfo target, int currentDeep)
        {
            currentDeep++;
            if (currentDeep > MaxRecursiveDeep)
            {
                throw new Exception("在导出文件时递归超过最大深度！");
            }

            // 复制所有顶层文件
            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            // 递归复制所有子目录
            foreach (DirectoryInfo sourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(sourceSubDir.Name);
                RecursiveCopy(sourceSubDir, nextTargetSubDir, currentDeep);
            }
        }
    }
}
