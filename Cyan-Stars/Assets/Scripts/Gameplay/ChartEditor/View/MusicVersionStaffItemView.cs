#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 动态创建。音乐版本中每一行作者信息持有一个 V。
    /// </summary>
    public class MusicVersionStaffItemView : BaseView<MusicVersionStaffItemViewModel>
    {
        [SerializeField]
        private TMP_InputField nameInputField = null!;

        [SerializeField]
        private TMP_InputField jobsInputField = null!;

        [SerializeField]
        private Button selectJobsButton = null!; //TODO

        [SerializeField]
        private Button deleteStaffItemButton = null!;


        public override void Bind(MusicVersionStaffItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.Name
                .Subscribe(nameString => nameInputField.text = nameString)
                .AddTo(this);
            jobsInputField.text = string.Join("/", ViewModel.Jobs);

            nameInputField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.UpdateName)
                .AddTo(this);
            jobsInputField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.UpdateJob)
                .AddTo(this);
            deleteStaffItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.DeleteItem())
                .AddTo(this);
        }

        protected override void OnDestroy()
        {
            // ISynchronizedView 会自动在 View 卸载时释放对应的 ViewModel，无需手动 ViewModel.Dispose();
        }
    }
}
