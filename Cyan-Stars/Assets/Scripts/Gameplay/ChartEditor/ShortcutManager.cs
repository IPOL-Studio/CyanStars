#nullable enable

using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 制谱器内快捷键管理器（单例）
    /// </summary>
    public class ShortcutManager : MonoBehaviour
    {
        [SerializeField]
        private CommandStack commandStack = null!;

        private ChartEditorModel model = null!;

        public void Init(ChartEditorModel chartEditorModel)
        {
            model = chartEditorModel;
        }

        void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                Input.GetKeyDown(KeyCode.Z))
            {
                // Ctrl+Z 撤销
                commandStack.Undo();
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                Input.GetKeyDown(KeyCode.Y))
            {
                // Ctrl+Y 重做
                commandStack.Redo();
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                Input.GetKeyDown(KeyCode.Z))
            {
                // Ctrl+Shift+Z 重做
                commandStack.Redo();
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && Input.GetKeyDown(KeyCode.S))
            {
                // Ctrl+S 保存
                ChartEditorFileManager.SaveChartAndAssetsToDesk(
                    model.WorkspacePath,
                    model.ChartMetaDataIndex,
                    model.ChartPackData.CurrentValue,
                    model.ChartData.CurrentValue
                );
            }
        }
    }
}
