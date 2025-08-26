using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using CyanStars.ChartEditor.Model;
using CyanStars.ChartEditor.View;
using UnityEngine;

namespace CyanStars.ChartEditor.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MainModel mainModel;

        // --- 常量 ---
        private const float BeatZoomStep = 0.1f; // 每次按下缩放加减按钮后，beatZoom 变化的值


        // --- 初始化默认值 ---
        private const bool DefaultPosMagnet = false;
        private const int DefaultPosPrecision = 4;
        private const int DefaultBeatPrecision = 2;
        private const float DefaultBeatZoom = 1;


        // --- EditToolbar ---

        private EditTools selectedEditTool;

        public EditTools SelectedEditTool
        {
            get => selectedEditTool;
            private set => SetField(ref selectedEditTool, value);
        }


        #region EditorAttribute

        /// <summary>
        /// 是否开启了位置吸附，不要用这个
        /// </summary>
        private bool posMagnet;

        /// <summary>
        /// 位置吸附属性
        /// </summary>
        public bool PosMagnet
        {
            get => posMagnet;
            set => SetField(ref posMagnet, value);
        }

        /// <summary>
        /// 真实的位置细分值，不要使用这个
        /// </summary>
        private int posPrecision;

        /// <summary>
        /// 位置细分输入框文本属性
        /// </summary>
        public string PosPrecisionInput
        {
            get => posPrecision.ToString();
            set
            {
                // 尝试解析和验证输入值
                if (int.TryParse(value, out int parsedValue) && parsedValue >= 0)
                {
                    // 验证通过，更新绑定的字符串属性的后端字段，然后更新真正的值
                    SetField(ref posPrecision, parsedValue);
                }
                else
                {
                    // 验证失败，通知 UI 刷新，强制输入框恢复到之前合法的值。
                    Debug.LogWarning($"MainViewModel: 输入的值 '{value}' 不合法，必须为非负整数。");
                    OnPropertyChanged(nameof(PosPrecisionInput));
                }
            }
        }

        /// <summary>
        /// 真实的节拍细分值，不要使用这个
        /// </summary>
        private int beatPrecision;

        /// <summary>
        /// 节拍细分输入框文本属性
        /// </summary>
        public string BeatPrecisionInput
        {
            get => beatPrecision.ToString();
            set
            {
                // 尝试解析和验证输入值
                if (int.TryParse(value, out int parsedValue) && parsedValue >= 1)
                {
                    // 验证通过，更新绑定的字符串属性的后端字段，然后更新真正的值
                    SetField(ref beatPrecision, parsedValue);
                }
                else
                {
                    // 验证失败，通知 UI 刷新，强制输入框恢复到之前合法的值。
                    Debug.LogWarning($"MainViewModel: 输入的值 '{value}' 不合法，必须为大于等于1的整数。");
                    OnPropertyChanged(nameof(BeatPrecisionInput));
                }
            }
        }

        /// <summary>
        /// 真实的节拍缩放值，不要使用这个
        /// </summary>
        private float beatZoom;

        /// <summary>
        /// 节拍缩放输入框文本属性
        /// </summary>
        public string BeatZoomInput
        {
            get => beatZoom.ToString(CultureInfo.InvariantCulture);
            set
            {
                // 尝试解析和验证输入值
                if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedValue) &&
                    parsedValue > 0)
                {
                    // 验证通过，更新绑定的字符串属性的后端字段，然后更新真正的值
                    SetField(ref beatZoom, parsedValue);
                }
                else
                {
                    // 验证失败，通知 UI 刷新，强制输入框恢复到之前合法的值。
                    Debug.LogWarning($"MainViewModel: 输入的值 '{value}' 不合法，必须为大于0的float。");
                    OnPropertyChanged(nameof(BeatZoomInput));
                }
            }
        }

        #endregion


        public MainViewModel(MainModel mainModel)
        {
            this.mainModel = mainModel;

            // 初始化各部分值
            PosMagnet = DefaultPosMagnet;
            posPrecision = DefaultPosPrecision;
            beatPrecision = DefaultBeatPrecision;
            beatZoom = DefaultBeatZoom;

            // TODO: 监听来自 Model 的事件
        }


        // --- EditToolbar ---

        /// <summary>
        /// 更新左侧画笔工具栏选中的工具
        /// </summary>
        public void ChangeEditTool(EditTools editTool)
        {
            SelectedEditTool = editTool;
        }


        // --- MenuButton ---

        /// <summary>
        /// 当左上角菜单窗口中的一级按钮被点击时
        /// </summary>
        public void MenuButtonClick(MenuButtons menuButton)
        {
            throw new NotSupportedException();
            // TODO: 完善点击响应逻辑
        }


        // --- EditorAttribute ---
        /// <summary>
        /// 减少节拍细分
        /// </summary>
        public void SubBeatPrecision()
        {
            int newValue = beatPrecision - 1;

            if (newValue <= 0)
            {
                // 如果新值小于或等于0，则不执行任何操作，保持当前值不变
                return;
            }

            BeatPrecisionInput = newValue.ToString();
        }

        /// <summary>
        /// 增加节拍细分
        /// </summary>
        public void AddBeatPrecision()
        {
            BeatPrecisionInput = (beatPrecision + 1).ToString();
        }


        /// <summary>
        /// 减少 BeatZoom 值（缩小）
        /// </summary>
        public void BeatZoomOut()
        {
            float newValue = beatZoom - BeatZoomStep;

            if (newValue <= 0)
            {
                // 如果新值小于或等于0，则不执行任何操作，保持当前值不变
                return;
            }

            BeatZoomInput = newValue.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 增加 BeatZoom 值（放大）
        /// </summary>
        public void BeatZoomIn()
        {
            BeatZoomInput = (beatZoom + BeatZoomStep).ToString(CultureInfo.InvariantCulture);
        }


        // --- MVVM 辅助方法 ---

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
