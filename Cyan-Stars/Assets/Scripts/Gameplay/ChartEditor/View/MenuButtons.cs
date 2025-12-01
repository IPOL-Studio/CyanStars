using CyanStars.GamePlay.ChartEditor.Model;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class MenuButtons : BaseView
    {
        /// <summary>
        /// 功能按钮实例
        /// </summary>
        [SerializeField]
        private Toggle functionToggle;

        /// <summary>
        /// 保存按钮实例
        /// </summary>
        [SerializeField]
        private Button saveButton;

        /// <summary>
        /// 测试按钮实例
        /// </summary>
        [SerializeField]
        private Button testButton;

        [Header("功能按钮相关")]
        [SerializeField]
        private Canvas functionCanvas;

        [SerializeField]
        private Button chartPackDataButton;

        [SerializeField]
        private Button chartDataButton;

        [SerializeField]
        private Button musicVersionButton;

        [SerializeField]
        private Button bpmGroupButton;

        [SerializeField]
        private Button speedGroupButton; // TODO

        [SerializeField]
        private Button exitSimplificationModeButton;

        [SerializeField]
        private Button enterSimplificationModeButton;


        public override void Bind(ChartEditorModel chartEditorModel)
        {
            base.Bind(chartEditorModel);

            functionToggle.onValueChanged.AddListener((isOn) =>
            {
                RefreshFunctionCanvas(isOn, Model.IsSimplification);
            });
            saveButton.onClick.AddListener(() => { Model.Save(); });
            chartPackDataButton.onClick.AddListener(() => { Model.SetChartPackDataCanvasVisibleness(true); });
            chartDataButton.onClick.AddListener(() => { Model.SetChartDataCanvasVisibleness(true); });
            musicVersionButton.onClick.AddListener(() => { Model.SetMusicVersionCanvasVisibleness(true); });
            bpmGroupButton.onClick.AddListener(() => { Model.SetBpmGroupCanvasVisibleness(true); });
            exitSimplificationModeButton.onClick.AddListener(() => { Model.SetSimplification(false); });
            enterSimplificationModeButton.onClick.AddListener(() => { Model.SetSimplification(true); });

            Model.OnSimplificationChanged += SimplificationChanged;
        }

        private void SimplificationChanged()
        {
            RefreshFunctionCanvas(functionToggle.isOn, Model.IsSimplification);
        }

        private void RefreshFunctionCanvas(bool isOn, bool isSimplificationMode)
        {
            functionCanvas.enabled = isOn;

            chartPackDataButton.gameObject.SetActive(true);
            chartDataButton.gameObject.SetActive(true);
            musicVersionButton.gameObject.SetActive(true);
            bpmGroupButton.gameObject.SetActive(true);
            speedGroupButton.gameObject.SetActive(false); // TODO
            exitSimplificationModeButton.gameObject.SetActive(isSimplificationMode);
            enterSimplificationModeButton.gameObject.SetActive(!isSimplificationMode);
        }

        private void OnDestroy()
        {
            Model.OnSimplificationChanged -= SimplificationChanged;
        }
    }
}
