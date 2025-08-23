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

            // TODO: 初始化文本框内容
            posPrecisionInputField.text = ViewModel.PosPrecisionInput;
            // ...

            // TODO: 添加双向绑定
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            posPrecisionInputField.onEndEdit.AddListener((string val) => { ViewModel.PosPrecisionInput = val; });
            // ...
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
            }
        }

        private void OnDestroy()
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }
    }
}
