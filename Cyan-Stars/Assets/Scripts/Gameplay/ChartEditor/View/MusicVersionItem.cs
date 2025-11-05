#nullable enable

using System;
using CyanStars.GamePlay.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    [RequireComponent(typeof(Button))]
    public class MusicVersionItem : BaseView
    {
        [SerializeField]
        private Button itemButton;

        [SerializeField]
        private GameObject itemLedObject;

        [SerializeField]
        private TMP_Text itemTitleText;


        private bool isInit = false;
        private int index;

        public void InitAndBind(ChartEditorModel chartEditorModel, int index)
        {
            isInit = true;
            this.index = index;
            Bind(chartEditorModel);
        }

        public override void Bind(ChartEditorModel chartEditorModel)
        {
            if (!isInit)
            {
                throw new Exception("MusicVersionItem: 未初始化数据");
            }

            base.Bind(chartEditorModel);

            Model.OnMusicVersionDataChanged -= RefreshUI;
            Model.OnMusicVersionDataChanged += RefreshUI;
            Model.OnSelectedMusicVersionItemChanged -= RefreshUI;
            Model.OnSelectedMusicVersionItemChanged += RefreshUI;

            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => { Model.SelectMusicVersionItem(index); });

            RefreshUI();
        }

        private void RefreshUI()
        {
            // 如果 Model 中选中编辑此 item，就启用 led
            itemLedObject.SetActive(Model.SelectedMusicVersionItemIndex != null &&
                                    (int)Model.SelectedMusicVersionItemIndex == index);

            itemTitleText.text = Model.MusicVersionDatas[index].VersionTitle;
        }

        public void OnDestroy()
        {
            Model.OnMusicVersionDataChanged -= RefreshUI;
            Model.OnSelectedMusicVersionItemChanged -= RefreshUI;
        }
    }
}
