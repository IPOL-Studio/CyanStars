using System;
using CyanStars.Chart;
using CyanStars.GamePlay.ChartEditor.Model;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
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

        private Button addItemButton;


        public override void Bind(ChartEditorModel chartEditorModel)
        {
            base.Bind(chartEditorModel);

            addItemButton = addItemButtonObject.GetComponent<Button>();
            closeCanvasButton.onClick.AddListener(() => { Model.SetMusicVersionCanvasVisibleness(false); });
            addItemButton.onClick.AddListener(() => { Model.AddMusicVersionItem(); });

            Model.OnMusicVersionDataChanged += RefreshUI;
            Model.OnMusicVersionCanvasVisiblenessChanged += RefreshUI;

            RefreshUI();
        }

        private void RefreshUI()
        {
            canvas.enabled = Model.MusicVersionCanvasVisibleness;

            addItemButtonObject.SetActive(!Model.IsSimplification);
            if (Model.MusicVersionDatas.Count == 0 && Model.IsSimplification)
            {
                Model.AddMusicVersionItem(new MusicVersionData());
            }


            // 删除多余元素
            MusicVersionItem[] items = contentObject.GetComponentsInChildren<MusicVersionItem>();
            for (int i = items.Length - 1; i >= Model.MusicVersionDatas.Count; i--)
            {
                Destroy(items[i].gameObject);
            }

            // 添加新元素
            for (int i = items.Length; i < Model.MusicVersionDatas.Count; i++)
            {
                GameObject go = Instantiate(musicVersionItemPrefab, contentObject.transform);
                go.transform.SetSiblingIndex(contentObject.transform.childCount - 2);
            }

            // 刷新已有元素的内容
            items = contentObject.GetComponentsInChildren<MusicVersionItem>();
            for (int i = 0; i < Model.MusicVersionDatas.Count; i++)
            {
                items[i].InitDataAndBind(Model, Model.MusicVersionDatas[i]);
            }

            // 刷新 UI 自动布局
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentObject.GetComponent<RectTransform>());
        }

        private void OnDestroy()
        {
            Model.OnMusicVersionDataChanged -= RefreshUI;
            Model.OnMusicVersionCanvasVisiblenessChanged -= RefreshUI;
        }
    }
}
