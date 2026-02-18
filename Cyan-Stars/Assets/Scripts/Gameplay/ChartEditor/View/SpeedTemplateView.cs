#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class SpeedTemplateView : BaseView<SpeedTemplateViewModel>
    {
        [SerializeField]
        private SpeedTemplateCurveFrameView speedTemplateCurveFrameView = null!;


        [SerializeField]
        private Canvas canvas = null!;

        [SerializeField]
        private Button closeCanvasButton = null!;

        [SerializeField]
        private GameObject speedTemplateItemPrefab = null!;

        [SerializeField]
        private Transform listContentTransform = null!;

        [SerializeField]
        private Button addSpeedTemplateItemButton = null!;

        [SerializeField]
        private GameObject detailFrameObject = null!;

        [SerializeField]
        private TMP_InputField remarkField = null!;

        [SerializeField]
        private Toggle relativeToggle = null!;

        [SerializeField]
        private Toggle absoluteToggle = null!;

        [SerializeField]
        private Button deleteButton = null!;

        [SerializeField]
        private Button cloneButton = null!;


        public override void Bind(SpeedTemplateViewModel targetViewModel)
        {
            base.Bind(targetViewModel);
            speedTemplateCurveFrameView.Bind(ViewModel.NewCurveFrameViewModel());

            closeCanvasButton.OnClickAsObservable()
                .Subscribe(_ => canvas.enabled = false)
                .AddTo(this);

            // 初始化时实例化已有的 item
            int index = 0;
            foreach (var vm in ViewModel.SpeedTemplateData)
            {
                var go = Instantiate(speedTemplateItemPrefab, listContentTransform);
                go.GetComponent<SpeedTemplateListItemView>().Bind(vm);
                go.transform.SetSiblingIndex(index);
                index++;
            }

            // VM->V 绑定列表元素
            ViewModel.SpeedTemplateData.ObserveAdd()
                .Subscribe(e =>
                    {
                        var go = Instantiate(speedTemplateItemPrefab, listContentTransform);
                        go.GetComponent<SpeedTemplateListItemView>().Bind(e.Value.View);
                        go.transform.SetSiblingIndex(e.Index);
                    }
                )
                .AddTo(this);
            ViewModel.SpeedTemplateData.ObserveRemove()
                .Subscribe(e =>
                    {
                        var itemToRemove = listContentTransform.GetChild(e.Index);
                        Destroy(itemToRemove.gameObject);
                    }
                )
                .AddTo(this);

            // VM->V 绑定编辑区
            Observable.Merge(
                    ViewModel.SelectedSpeedTemplateData,
                    ViewModel.SelectedSpeedTemplateDataPropertyUpdatedSubject.Select(data => (SpeedTemplateData?)data)
                )
                .Subscribe(data =>
                    {
                        if (data == null)
                        {
                            detailFrameObject.SetActive(false);
                            return;
                        }

                        detailFrameObject.SetActive(true);
                        remarkField.text = data.Remark;
                        if (data.Type == SpeedTemplateType.Relative) // Unity ToggleGroup 会自动关掉另一个
                            relativeToggle.isOn = true;
                        else
                            absoluteToggle.isOn = true;
                    }
                )
                .AddTo(this);

            // V->VM 绑定
            addSpeedTemplateItemButton.OnClickAsObservable()
                .Subscribe(_ => ViewModel.AddSpeedTemplateData())
                .AddTo(this);
            deleteButton.OnClickAsObservable()
                .Subscribe(_ => ViewModel.DeleteSelectedSpeedTemplateData())
                .AddTo(this);
            cloneButton.OnClickAsObservable()
                .Subscribe(_ => ViewModel.CloneSelectedSpeedTemplateData())
                .AddTo(this);
        }

        public void OpenCanvas()
        {
            canvas.enabled = true;
        }
    }
}
