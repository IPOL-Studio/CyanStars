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

            listItems = listContent.GetComponentsInChildren<SpeedGroupListItem>();

            for (int i = 0; i < listItems.Length; i++)
            {
                listItems[i].Toggle.isOn = (ViewModel.CurrentSpeedGroupIndex == i);
            }

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
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
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentSpeedGroupIndex):
                    for (int i = 0; i < listItems.Length; i++)
                    {
                        listItems[i].Toggle.isOn = (ViewModel.CurrentSpeedGroupIndex == i);
                    }

                    break;

                case nameof(ViewModel.SpeedGroupRemarkInput):
                    listItems[ViewModel.CurrentSpeedGroupIndex].RemarkText.text = ViewModel.SpeedGroupRemarkInput;

                    break;
            }
            // TODO: 在删除、复制、添加元素后刷新当前选中的列表元素
        }

        public void Destroy()
        {
            // TODO: 取消订阅
        }
    }
}
