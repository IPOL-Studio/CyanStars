using CyanStars.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class SpeedTemplateEditor : BaseView
    {
        [SerializeField]
        private TMP_Text indexText;

        [SerializeField]
        private TMP_InputField remarkInput;

        [SerializeField]
        private Button relativeButton;

        [SerializeField]
        private Button absoluteButton;

        [SerializeField]
        private Button delItemButton;

        [SerializeField]
        private Button copyItemButton;

        public override void Bind(MainViewModel mainViewModel)
        {
            base.Bind(mainViewModel);
        }

        private void OnViewModelPropertyChanged(object _, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO: 在 SpeedTemplateList 切换、添加元素，或在自身删除、复制元素后刷新编辑器 UI
        }

        public void Destroy()
        {
            // TODO: 取消订阅
        }
    }
}
