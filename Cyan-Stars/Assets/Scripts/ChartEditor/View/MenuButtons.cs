using CyanStars.ChartEditor.Model;
using CyanStars.ChartEditor.View;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChatrEditor.View
{
    public class MenuButtons : BaseView
    {
        /// <summary>
        /// 功能按钮实例
        /// </summary>
        [SerializeField]
        private Button FunctionButton;

        /// <summary>
        /// 保存按钮实例
        /// </summary>
        [SerializeField]
        private Button SaveButton;

        /// <summary>
        /// 测试按钮实例
        /// </summary>
        [SerializeField]
        private Button TestButton;

        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            FunctionButton.onClick.AddListener(() => { Model.MenuButtonClicked(MenuButton.FunctionButton); });
            SaveButton.onClick.AddListener(() => { Model.MenuButtonClicked(MenuButton.SaveButton); });
            TestButton.onClick.AddListener(() => { Model.MenuButtonClicked(MenuButton.TestButton); });
        }
    }
}
