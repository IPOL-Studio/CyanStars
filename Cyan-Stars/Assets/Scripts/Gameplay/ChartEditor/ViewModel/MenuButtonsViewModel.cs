#nullable enable

using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MenuButtonsViewModel : BaseViewModel
    {
        public ReadOnlyReactiveProperty<bool> IsSimplificationMode => Model.IsSimplificationMode;


        public MenuButtonsViewModel(ChartEditorModel model)
            : base(model)
        {
        }


        public void SetSimplificationMode(bool newValue)
        {
            if (newValue == Model.IsSimplificationMode.Value)
                return;

            CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        Model.IsSimplificationMode.Value = newValue;
                    },
                    () =>
                    {
                        Model.IsSimplificationMode.Value = !newValue;
                    }
                )
            );
        }

        public void Undo()
        {
            base.CommandStack.Undo();
        }

        public void Redo()
        {
            base.CommandStack.Redo();
        }

        public void SaveFileToDesk()
        {
            ChartEditorFileManager.SaveChartAndAssetsToDesk(
                Model.WorkspacePath,
                Model.ChartMetaDataIndex,
                Model.ChartPackData.CurrentValue,
                Model.ChartData.CurrentValue
            );
        }
    }
}
