using CyanStars.Chart;
using CyanStars.ChartEditor.Model;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class MusicVersionCanvas : BaseView
    {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private Button closeCanvasButton;

        [SerializeField]
        private GameObject musicVersionItemPrefab;

        [SerializeField]
        private GameObject contentObject;

        [SerializeField]
        private GameObject addItemButtonObject;

        private Button AddItemButton => addItemButtonObject.GetComponent<Button>();


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            foreach (MusicVersionData musicVersionData in Model.MusicVersionDatas)
            {
                GameObject go = Instantiate(musicVersionItemPrefab, contentObject.transform);
                go.transform.SetSiblingIndex(contentObject.transform.childCount - 2);
                go.GetComponent<MusicVersionItem>().InitData(Model, musicVersionData);
            }
        }
    }
}
