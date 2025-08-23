using CyanStars.ChartEditor.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public enum MenuButtons
    {
        Menu,
        Save,
        Play
    }

    /// <summary>
    /// 左上角菜单 view 层，用于控制按钮
    /// </summary>
    public class MenuCanvas : BaseView
    {
        [SerializeField]
        private Button menuButton;

        [SerializeField]
        private Button saveButton;

        [SerializeField]
        private Button playButton;


        public override void Bind(MainViewModel mainViewModel)
        {
            base.Bind(mainViewModel);

            menuButton.onClick.AddListener(() => { ViewModel.MenuButtonClick(MenuButtons.Menu); });
            saveButton.onClick.AddListener(() => { ViewModel.MenuButtonClick(MenuButtons.Save); });
            playButton.onClick.AddListener(() => { ViewModel.MenuButtonClick(MenuButtons.Play); });
        }

        private void OnDestroy()
        {
            menuButton.onClick.RemoveAllListeners();
            saveButton.onClick.RemoveAllListeners();
            playButton.onClick.RemoveAllListeners();
        }
    }
}
