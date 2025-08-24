using CyanStars.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class EditorAttributeCanvas : BaseView
    {
        // --- 位置细分 ---

        [SerializeField]
        private TMP_InputField posPrecisionInputField;

        [SerializeField]
        private Toggle posMagnetToggle;

        // --- 节拍细分 ---

        [SerializeField]
        private TMP_InputField beatPrecisionInputField;

        [SerializeField]
        private Button beatPrecisionSubBuutton;

        [SerializeField]
        private Button beatPrecisionAddBuutton;

        // --- 节拍缩放 ---

        [SerializeField]
        private TMP_InputField beatZoomInputField;

        [SerializeField]
        private Button beatZoomOutButton; // 压缩纵向长度，看得更多

        [SerializeField]
        private Button beatZoomInButton; // 拉伸纵向长度，看得更细


        public override void Bind(MainViewModel mainViewModel)
        {
            base.Bind(mainViewModel);

            // 初始化文本框内容
            posPrecisionInputField.text = ViewModel.PosPrecisionInput;
            beatPrecisionInputField.text = ViewModel.BeatPrecisionInput;
            beatZoomInputField.text = ViewModel.BeatZoomInput;

            // 添加双向绑定
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            posPrecisionInputField.onEndEdit.AddListener((string val) => { ViewModel.PosPrecisionInput = val; });
            beatPrecisionInputField.onEndEdit.AddListener((string val) => { ViewModel.BeatPrecisionInput = val; });
            beatZoomInputField.onEndEdit.AddListener((string val) => { ViewModel.BeatZoomInput = val; });
            // TODO: 为按钮添加绑定
        }

        /// <summary>
        /// 当监听到 ViewModel 变化事件时，分析事件并更新 object
        /// </summary>
        private void OnViewModelPropertyChanged(object _, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.PosPrecisionInput):
                    if (posPrecisionInputField.text != ViewModel.PosPrecisionInput) // 防止无限循环和避免编辑时光标跳动
                    {
                        posPrecisionInputField.text = ViewModel.PosPrecisionInput;
                    }

                    break;
                case nameof(MainViewModel.BeatPrecisionInput):
                    if (beatPrecisionInputField.text != ViewModel.BeatPrecisionInput)
                    {
                        beatPrecisionInputField.text = ViewModel.BeatPrecisionInput;
                    }

                    break;
                case nameof(MainViewModel.BeatZoomInput):
                    if (beatZoomInputField.text != ViewModel.BeatZoomInput)
                    {
                        beatZoomInputField.text = ViewModel.BeatZoomInput;
                    }

                    break;
            }
        }

        private void OnDestroy()
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            posPrecisionInputField.onEndEdit.RemoveAllListeners();
            beatPrecisionInputField.onEndEdit.RemoveAllListeners();
            beatZoomInputField.onEndEdit.RemoveAllListeners();
        }
    }
}
