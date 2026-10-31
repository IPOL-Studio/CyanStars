#nullable enable

using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.File;
using CyanStars.Framework.File.Transfer;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Management;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Utils;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartPackDataViewModel : BaseViewModel
    {
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
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.CoverFilePath.Value ?? "")
                .AddTo(base.Disposables);

            ChartPackInfo = Model.ChartPackData
                .Select(data => data.ChartPackInfo.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.ChartPackInfo.Value)
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
            if (!GameRoot.File.Environment.CanExportFolder)
            {
                PopupView.ShowError("无法导出谱包", "当前平台不支持导出谱包。");
                return;
            }

            // TODO: 将导出的文件打包为一个专有后缀名的文件
            if (!GameRoot.File.TryOpenSaveFolderPathBrowser(ExportChartPackTo, null, "导出到"))
                PopupView.ShowError("无法导出谱包", "无法打开文件夹选择对话框，请稍后重试。");
        }

        /// <summary>
        /// 把当前谱包保存到工作区后，整体复制到玩家选中的目标位置
        /// </summary>
        private void ExportChartPackTo(string targetParentPath)
        {
            // 1. 先在内存中固定和校验导出的目标数据
            string folderName = PathUtil.GetName(Model.WorkspacePath);
            string destPath = PathUtil.Combine(targetParentPath, folderName);

            if (!GameRoot.File.TryValidateFolderTransfer(
                    Model.WorkspacePath,
                    destPath,
                    FolderTransferDirection.Export,
                    out string error))
            {
                PopupView.ShowError("无法导出谱包", error);
                return;
            }

            // 2. 再把当前谱包保存到玩家数据路径，保存失败则取消导出
            if (!ChartEditorSaver.SaveChartAndAssetsToDisk(Model, GameRoot.File))
            {
                PopupView.ShowError("无法导出谱包", "保存谱包数据失败，已取消导出。具体原因见日志。");
                return;
            }

            // 保存成功，工作区数据已与磁盘一致
            CommandStack.MarkSaved();

            // 3. 最后把工作区复制到指定路径，已存在同名文件夹时依次添加 "(1)" 等后缀
            if (!GameRoot.File.TryExportFolder(Model.WorkspacePath, destPath, out _, out string exportError))
            {
                // 校验类失败有明确原因；复制阶段失败的原因见日志
                PopupView.ShowError(
                    "无法导出谱包",
                    string.IsNullOrEmpty(exportError) ? "复制谱包文件失败，具体原因见日志。" : exportError);
            }
        }
    }
}
