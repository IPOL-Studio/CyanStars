using CyanStars.ChartEditor.Model;
using CyanStars.ChartEditor.View;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChatrEditor.View
{
    public class MenuButtons : BaseView
    {
        /// <summary>
        /// 功能按钮实例
        /// </summary>
        [SerializeField]
        private Toggle FunctionToggle;

        /// <summary>
        /// 保存按钮实例
        /// </summary>
        [SerializeField]
        private Button SaveButton;

        /// <summary>
        /// 测试按钮实例
        /// </summary>
        [SerializeField]
        private Button TestButton;

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
        private Button entrySimplificationModeButton;

        [SerializeField]
        private Button exitSimplificationModeButton;


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            FunctionToggle.onValueChanged.AddListener((isOn) =>
            {
                RefreshFunctionCanvas(isOn, Model.IsSimplification);
            });
            chartPackDataButton.onClick.AddListener(() => { Model.SetChartPackDataCanvasVisibleness(true); });
            chartDataButton.onClick.AddListener(() => { Model.SetChartDataCanvasVisibleness(true); });
            musicVersionButton.onClick.AddListener(() => { Model.SetMusicVersionCanvasVisibleness(true); });
            bpmGroupButton.onClick.AddListener(() => { Model.SetBpmGroupCanvasVisibleness(true); });
            Model.OnSimplificationChanged += SimplificationChanged;
        }

        private void SimplificationChanged()
        {
            RefreshFunctionCanvas(FunctionToggle.isOn, Model.IsSimplification);
        }

        private void RefreshFunctionCanvas(bool isOn, bool isSimplificationMode)
        {
            functionCanvas.enabled = isOn;

            chartPackDataButton.gameObject.SetActive(true);
            chartDataButton.gameObject.SetActive(true);
            musicVersionButton.gameObject.SetActive(!isSimplificationMode);
            bpmGroupButton.gameObject.SetActive(true);
            speedGroupButton.gameObject.SetActive(false); // TODO
            entrySimplificationModeButton.gameObject.SetActive(!isSimplificationMode);
            exitSimplificationModeButton.gameObject.SetActive(isSimplificationMode);
        }

        private void OnDestroy()
        {
            Model.OnSimplificationChanged -= SimplificationChanged;
        }
    }
}
