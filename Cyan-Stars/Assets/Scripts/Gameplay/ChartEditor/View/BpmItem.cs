using System;
using CyanStars.Chart;
using CyanStars.GamePlay.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class BpmItem : BaseView
    {
        private bool isInit = false;
        private int index;


        [SerializeField]
        private Image led;

        [SerializeField]
        private TMP_Text indexText;

        [SerializeField]
        private TMP_Text detailText;

        [SerializeField]
        private Button button;


        public void InitDataAndBind(EditorModel editorModel, int index)
        {
            isInit = true;
            this.index = index;
            Bind(editorModel);
        }

        public override void Bind(EditorModel editorModel)
        {
            if (!isInit)
            {
                throw new Exception("BpmItem: Bind called on uninitialized view");
            }

            base.Bind(editorModel);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { Model.SelectBpmItem(index); });

            RefreshUI();
        }

        public void RefreshUI()
        {
            led.enabled = (Model.SelectedBpmGroupIndex == index);
            indexText.text = index.ToString();

            Beat beat = Model.BpmGroupDatas[index].StartBeat;
            int msTime = Model.ChartData.BpmGroup.CalculateTime(Model.BpmGroupDatas[index].StartBeat);
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(msTime); // 应该没人会写总时长超过 24 小时的变速谱面
            detailText.text = $"[{beat.IntegerPart}, {beat.Numerator}, {beat.Denominator}]" +
                              $"\n{timeSpan:hh\\:mm\\:ss\\.fff}";
        }
    }
}
