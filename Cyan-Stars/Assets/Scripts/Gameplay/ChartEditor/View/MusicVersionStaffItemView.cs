#nullable enable

using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        nameInputField.text = ViewModel.Name;
        jobsInputField.text = string.Join("/", ViewModel.Jobs);

        nameInputField.onEndEdit.AddListener(ViewModel.UpdateName);
        jobsInputField.onEndEdit.AddListener(ViewModel.UpdateJob);
        deleteStaffItemButton.onClick.AddListener(ViewModel.DeleteItem);
    }

    protected override void OnDestroy()
    {
        if (ViewModel == null)
            return;

        nameInputField.onEndEdit.RemoveListener(ViewModel.UpdateName);
        jobsInputField.onEndEdit.RemoveListener(ViewModel.UpdateJob);
        deleteStaffItemButton.onClick.RemoveListener(ViewModel.DeleteItem);
        // ISynchronizedView 会自动在 View 卸载时释放对应的 ViewModel，无需手动 ViewModel.Dispose();
    }
}
