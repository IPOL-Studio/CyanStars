#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor
{
    public class Background : MonoBehaviour, IPointerClickHandler
    {
        private ChartEditorModel? model;

        public void Init(ChartEditorModel chartEditorModel)
        {
            model = chartEditorModel;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (model is null)
            {
                Debug.LogWarning("model 尚未初始化，取消音符操作无效。");
                return;
            }

            model.SelectedNoteData.Value = null;
        }
    }
}
