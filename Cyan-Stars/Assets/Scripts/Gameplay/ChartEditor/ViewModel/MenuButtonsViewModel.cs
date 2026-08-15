#nullable enable

using CyanStars.Framework;
using CyanStars.Gameplay.Base;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Management;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MenuButtonsViewModel : BaseViewModel
    {
        public MenuButtonsViewModel(ChartEditorModel model)
            : base(model)
        {
        }

        public void ExitChartEditor() => GameRoot.ChangeProcedure<MainHomeProcedure>();

        public void Undo() => base.CommandStack.Undo();
        public void Redo() => base.CommandStack.Redo();

        /// <summary>
        /// 是否存在未保存数据
        /// </summary>
        public ReadOnlyReactiveProperty<bool> HasUnsavedChanges => CommandStack.HasUnsavedChanges;

        /// <summary>
        /// 当前是否可以撤销
        /// </summary>
        public ReadOnlyReactiveProperty<bool> CanUndo => CommandStack.CanUndo;

        /// <summary>
        /// 当前是否可以重做
        /// </summary>
        public ReadOnlyReactiveProperty<bool> CanRedo => CommandStack.CanRedo;

        public void SaveFileToDisk()
        {
            bool isSaveSuccess = ChartEditorFileManager.SaveChartAndAssetsToDisk(
                Model.WorkspacePath,
                Model.ChartMetaDataIndex,
                Model.ChartPackData.CurrentValue,
                Model.ChartData.CurrentValue
            );

            // 只有保存成功才标记为已保存，失败保持未保存状态便于重试
            if (isSaveSuccess)
                CommandStack.MarkSaved();
        }
    }
}
