using CyanStars.ChartEditor.ViewHelper;
using CyanStars.ChartEditor.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class SpeedGroupList : BaseView
    {
        [SerializeField]
        private GameObject listContent;

        [SerializeField]
        private Button addItemButton;

        private SpeedGroupListItem[] listItems;


        public override void Bind(MainViewModel mainViewModel)
        {
            base.Bind(mainViewModel);

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            listItems = listContent.GetComponentsInChildren<SpeedGroupListItem>();
            for (int i = 0; i < listItems.Length; i++)
            {
                int index = i;
                listItems[i].Toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        mainViewModel.ChangeSpeedGroup(index);
                    }
                });
            }

            addItemButton.onClick.AddListener(mainViewModel.AddSpeedGroup);
        }

        private void OnViewModelPropertyChanged(object _, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO: 在 SpeedTemplateEditor View 删除元素后刷新当前选中的列表元素
            // TODO: 在 SpeedTemplateEditor View 更新元素后更新列表元素文本
        }

        public void Destroy()
        {
            // TODO: 取消订阅
        }
    }
}
