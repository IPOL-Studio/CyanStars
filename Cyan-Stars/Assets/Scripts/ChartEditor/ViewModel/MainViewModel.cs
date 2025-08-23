using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        // --- EditToolbar ---

        private EditTools selectedEditTool;

        public EditTools SelectedEditTool
        {
            get => selectedEditTool;
            private set => SetField(ref selectedEditTool, value);
        }

        public MainViewModel(MainModel mainModel)
        {
            this.mainModel = mainModel;
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
