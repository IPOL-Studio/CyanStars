using System;
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


        private Button addItemButton;


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            addItemButton = addItemButtonObject.GetComponent<Button>();

            Model.OnMusicVersionDataChanged += RefreshUI;
            addItemButton.onClick.AddListener(() => { Model.AddMusicVersionItem(); });

            RefreshUI();
        }

        private void RefreshUI()
        {
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

            // 刷新已有元素的内容
            items = contentObject.GetComponentsInChildren<MusicVersionItem>();
            for (int i = 0; i < items.Length; i++)
            {
                items[i].InitDataAndBind(Model, Model.MusicVersionDatas[i]);
            }

            // 添加并刷新新元素
            for (int i = items.Length; i < Model.MusicVersionDatas.Count; i++)
            {
                GameObject go = Instantiate(musicVersionItemPrefab, contentObject.transform);
                go.transform.SetSiblingIndex(contentObject.transform.childCount - 2);
                go.GetComponent<MusicVersionItem>().InitDataAndBind(Model, Model.MusicVersionDatas[i]);
            }

            // 刷新 UI 自动布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentObject.GetComponent<RectTransform>());
        }

        private void OnDestroy()
        {
            Model.OnMusicVersionDataChanged -= RefreshUI;
        }
    }
}
