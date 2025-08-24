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

        // --- 初始化默认值 ---

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
        /// 真实的位置细分值，不要使用这个
        /// </summary>
        private int posPrecision;

        /// <summary>
        /// 位置细分属性
        /// </summary>
        public int PosPrecision
        {
            get => posPrecision;
            private set => SetField(ref posPrecision, value);
        }

        /// <summary>
        /// 有效的位置细分输入框文本，不要使用这个
        /// </summary>
        private string posPrecisionInput;

        /// <summary>
        /// 位置细分输入框文本属性
        /// </summary>
        public string PosPrecisionInput
        {
            get => posPrecisionInput;
            set
            {
                // 尝试解析和验证输入值
                if (int.TryParse(value, out int parsedValue) && parsedValue >= 0)
                {
                    // 验证通过，更新绑定的字符串属性的后端字段，然后更新真正的值
                    SetField(ref posPrecisionInput, value);
                    if (PosPrecision != parsedValue)
                    {
                        PosPrecision = parsedValue;
                    }
                }
                else
                {
                    // 验证失败，通知 UI 刷新，强制输入框恢复到之前合法的值。
                    Debug.LogWarning($"MainViewModel: 输入的值 '{value}' 不合法，必须为非负整数。");
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 真实的节拍细分值，不要使用这个
        /// </summary>
        private int beatPrecision;

        /// <summary>
        /// 节拍细分属性
        /// </summary>
        public int BeatPrecision
        {
            get => beatPrecision;
            private set => SetField(ref beatPrecision, value);
        }

        /// <summary>
        /// 有效的节拍细分输入框文本，不要使用这个
        /// </summary>
        private string beatPrecisionInput;

        /// <summary>
        /// 节拍细分输入框文本属性
        /// </summary>
        public string BeatPrecisionInput
        {
            get => beatPrecisionInput;
            set
            {
                // 尝试解析和验证输入值
                if (int.TryParse(value, out int parsedValue) && parsedValue >= 1)
                {
                    // 验证通过，更新绑定的字符串属性的后端字段，然后更新真正的值
                    SetField(ref beatPrecisionInput, value);
                    if (BeatPrecision != parsedValue)
                    {
                        BeatPrecision = parsedValue;
                    }
                }
                else
                {
                    // 验证失败，通知 UI 刷新，强制输入框恢复到之前合法的值。
                    Debug.LogWarning($"MainViewModel: 输入的值 '{value}' 不合法，必须为大于等于1的整数。");
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 真实的节拍缩放值，不要使用这个
        /// </summary>
        private float beatZoom;

        /// <summary>
        /// 节拍缩放属性
        /// </summary>
        public float BeatZoom
        {
            get => beatZoom;
            private set => SetField(ref beatZoom, value);
        }

        /// <summary>
        /// 有效的节拍缩放输入框文本，不要使用这个
        /// </summary>
        private string beatZoomInput;

        /// <summary>
        /// 节拍缩放输入框文本属性
        /// </summary>
        public string BeatZoomInput
        {
            get => beatZoomInput;
            set
            {
                // 尝试解析和验证输入值
                if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedValue) &&
                    parsedValue > 0)
                {
                    // 验证通过，更新绑定的字符串属性的后端字段，然后更新真正的值
                    SetField(ref beatZoomInput, value);
                    if (!Mathf.Approximately(BeatZoom, parsedValue))
                    {
                        BeatZoom = parsedValue;
                    }
                }
                else
                {
                    // 验证失败，通知 UI 刷新，强制输入框恢复到之前合法的值。
                    Debug.LogWarning($"MainViewModel: 输入的值 '{value}' 不合法，必须为大于0的float。");
                    OnPropertyChanged();
                }
            }
        }

        #endregion


        public MainViewModel(MainModel mainModel)
        {
            this.mainModel = mainModel;

            // 初始化各部分值
            posPrecision = DefaultPosPrecision;
            posPrecisionInput = DefaultPosPrecision.ToString();
            beatPrecision = DefaultBeatPrecision;
            beatPrecisionInput = DefaultBeatPrecision.ToString();
            beatZoom = DefaultBeatZoom;
            beatZoomInput = DefaultBeatZoom.ToString(CultureInfo.InvariantCulture);

            // TODO: 监听来自 Model 的事件
        }

        // TODO: 为按钮添加绑定方法


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


        // --- MVVM 辅助方法 ---

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
