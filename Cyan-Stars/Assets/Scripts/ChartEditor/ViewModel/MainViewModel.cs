using System;
using System.ComponentModel;
using CyanStars.ChartEditor.Model;
using CyanStars.ChartEditor.View;

namespace CyanStars.ChartEditor.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MainModel mainModel;


        private EditTools SelectedEditTool;


        public MainViewModel(MainModel mainModel)
        {
            this.mainModel = mainModel;
            // TODO: 监听来自 Model 的事件
        }

        /// <summary>
        /// 更新左侧画笔工具栏选中的工具
        /// </summary>
        public void ChangeEditTool(EditTools editTool)
        {
            SelectedEditTool = editTool;
        }

        /// <summary>
        /// 当左上角菜单窗口中的一级按钮被点击时
        /// </summary>
        public void MenuButtonClick(MenuButtons menuButton)
        {
            throw new NotSupportedException();
            // TODO: 完善点击响应逻辑
        }

        // protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        // {
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // }
        //
        // protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        // {
        //     if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        //     field = value;
        //     OnPropertyChanged(propertyName);
        //     return true;
        // }
    }
}
