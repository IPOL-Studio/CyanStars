using System;
using CyanStars.Chart;
using UnityEngine;
using CyanStars.ChartEditor.Model;
using CyanStars.Framework;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class EditArea : BaseView
    {
        /// <summary>
        /// 一倍缩放时整数节拍线之间的间隔距离
        /// </summary>
        private const float DefaultBeatLineInterval = 250f;

        private const string BeatLinePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/BeatLine.prefab";


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
        private RectTransform JudgeLineRect => judgeLine.GetComponent<RectTransform>();

        private int totalBeats; // 当前选中的音乐（含offset）的向上取整拍子总数量
        private int lastScreenHeight; // 上次记录的屏幕高度（刷新前）
        private float contentHeight; // 内容总高度


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);
            ResetTotalBeats();
        }

        private void Start()
        {
            RefreshUI();
        }

        /// <summary>
        /// 画面变化后(而不是每帧)或在编辑器或音符属性修改后，重新绘制编辑器的节拍线、位置线、音符
        /// </summary>
        private void RefreshUI()
        {
            RefreshScrollRect();
            RefreshBeatLine();
        }

        private async void RefreshBeatLine()
        {
            // 归还所有节拍线到池
            foreach (Transform childTransform in beatLines.transform)
            {
                GameRoot.GameObjectPool.ReleaseGameObject(BeatLinePrefabPath, childTransform.gameObject);
            }

            // 计算屏幕下沿对应的 content 位置
            float contentPos = (ContentRect.sizeDelta.y - Screen.height) * scrollRect.verticalNormalizedPosition;

            // 计算每条节拍线（包括细分节拍线）占用的位置
            float beatLineDistance = DefaultBeatLineInterval * Model.BeatZoom * Model.BeatAccuracy;

            // 计算第一条需要渲染的节拍线的计数
            int currentBeatLineCount =
                (int)((contentPos - JudgeLineRect.position.y) / beatLineDistance); // 屏幕外会多渲染几条节拍线，符合预期
            currentBeatLineCount = Math.Max(1, currentBeatLineCount);

            while ((currentBeatLineCount - 1) * beatLineDistance < contentPos + Screen.height)
            {
                // 渲染节拍线
                GameObject go =
                    await GameRoot.GameObjectPool.GetGameObjectAsync(BeatLinePrefabPath, beatLines.transform);
                BeatLine beatLine = go.GetComponent<BeatLine>();
                RectTransform rect = beatLine.BeatLineRect;
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(
                    0,
                    JudgeLineRect.position.y + beatLineDistance * (currentBeatLineCount - 1) - contentPos
                );

                int beatAccNum = (currentBeatLineCount - 1) % Model.BeatAccuracy;
                if (beatAccNum == 0)
                {
                    // 整数节拍线
                    beatLine.Image.color = Color.white;
                    beatLine.BeatTextObject.SetActive(true);
                    beatLine.BeatText.text = currentBeatLineCount.ToString();
                }
                else if (Model.BeatAccuracy % 2 == 0 && beatAccNum == Model.BeatAccuracy / 2)
                {
                    // 1/2 节拍线
                    beatLine.Image.color = new Color(0.7f, 0.6f, 1f, 0.8f);
                    beatLine.BeatTextObject.SetActive(false);
                }
                else if (Model.BeatAccuracy % 4 == 0 &&
                         (beatAccNum == Model.BeatAccuracy / 4 || beatAccNum == Model.BeatAccuracy / 4 * 3))
                {
                    // 1/4 或 3/4 节拍线
                    beatLine.Image.color = new Color(0.5f, 0.8f, 1f, 0.8f);
                    beatLine.BeatTextObject.SetActive(false);
                }
                else
                {
                    // 其他节拍线
                    beatLine.Image.color = new Color(0.6f, 1f, 0.6f, 0.8f);
                    beatLine.BeatTextObject.SetActive(false);
                }

                currentBeatLineCount++;
            }
        }

        private void RefreshScrollRect()
        {
            // 记录已滚动的位置百分比
            float verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;

            // 刷新 content 高度
            contentHeight = Math.Max(
                totalBeats * DefaultBeatLineInterval * Model.BeatZoom + JudgeLineRect.transform.position.y,
                Screen.height
            );
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
