using System;
using CyanStars.Chart;
using UnityEngine;
using CyanStars.ChartEditor.Model;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class EditArea : BaseView
    {
        /// <summary>
        /// 一倍缩放时整数节拍线之间的间隔距离
        /// </summary>
        private const float DefaultBeatLineInterval = 250f;


        [SerializeField]
        private GameObject tracks;

        [SerializeField]
        private GameObject posLines;

        [SerializeField]
        private GameObject beatLines;

        [SerializeField]
        private GameObject judgeLine;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GameObject notes;

        private RectTransform ContentRect => scrollRect.content.GetComponent<RectTransform>();

        private int totalBeats; // 当前选中的音乐（含offset）的向上取整拍子总数量
        private int lastScreenHeight; // 上次记录的屏幕高度
        private float contentHeight; // 内容总高度
        private float contentPosition; // 内容纵向位置（内容下边界对齐屏幕下边界）


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);
            ResetTotalBeats();
        }

        /// <summary>
        /// 画面变化后(而不是每帧)或在编辑器或音符属性修改后，重新绘制编辑器的节拍线、位置线、音符
        /// </summary>
        private async void RefreshUI()
        {
            // 记录已滚动的位置百分比
            float verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;

            // 刷新 content 高度
            contentHeight = Math.Max(totalBeats * DefaultBeatLineInterval * Model.BeatZoom, Screen.height);
            ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x, contentHeight);

            // 恢复 content 位置
            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
        }

        /// <summary>
        /// 重计算总拍数并刷新编辑器视图
        /// </summary>
        private void ResetTotalBeats()
        {
            totalBeats = CalculateTotalBeats(Model.ActualMusicTime, Model.ChartData.BpmGroup);
            RefreshUI();
        }

        /// <summary>
        /// 根据总时间和 bpm 组数据，计算并向上取整总共有几个拍子。
        /// <para>将计算已经经过的拍子时，不要忘了 -1</para>
        /// </summary>
        /// <param name="actualMusicTime">计算 offset 后的音乐总时长（ms）</param>
        /// <param name="bpmGroup">bpm 组数据</param>
        /// <returns>音乐总共有几拍（向上取整）</returns>
        private int CalculateTotalBeats(int actualMusicTime, BpmGroup bpmGroup)
        {
            bpmGroup.SortGroup();

            // 懒得写算法了，暴力枚举吧
            int beatCount = 0;
            while (bpmGroup.CalculateTime(beatCount) < actualMusicTime)
            {
                beatCount++;
            }

            return beatCount;
        }

        private void Update()
        {
            // TODO: 改为由 GameRoot 下发事件
            if (lastScreenHeight != Screen.height)
            {
                RefreshUI();
                lastScreenHeight = Screen.height;
            }

            RefreshUI();
        }
    }
}
