#nullable enable

using System;
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
            foreach (var vm in ViewModel.SpeedTemplateDatas)
            {
                var go = Instantiate(speedTemplateItemPrefab, listContentTransform);
                go.GetComponent<SpeedTemplateListItemView>().Bind(vm);
                go.transform.SetSiblingIndex(index);
                index++;
            }

            // VM->V 绑定列表元素
            ViewModel.SpeedTemplateDatas.ObserveAdd()
                .Subscribe(e =>
                    {
                        var go = Instantiate(speedTemplateItemPrefab, listContentTransform);
                        go.GetComponent<SpeedTemplateListItemView>().Bind(e.Value.View);
                        go.transform.SetSiblingIndex(e.Index);
                    }
                )
                .AddTo(this);
            ViewModel.SpeedTemplateDatas.ObserveRemove()
                .Subscribe(e =>
                    {
                        var itemToRemove = listContentTransform.GetChild(e.Index);
                        Destroy(itemToRemove.gameObject);
                    }
                )
                .AddTo(this);

            // VM->V 绑定编辑区
            ViewModel.SelectedSpeedTemplateData
                .Subscribe(selectedSpeedTemplate => detailFrameObject.SetActive(selectedSpeedTemplate != null))
                .AddTo(this);
            ViewModel.SelectedSpeedTemplateData
                .Select(selectedSpeedTemplate => selectedSpeedTemplate?.Remark ?? Observable.Empty<string>())
                .Switch()
                .Subscribe(remark => remarkField.text = remark)
                .AddTo(this);
            ViewModel.SelectedSpeedTemplateData
                .Select(selectedSpeedTemplate => selectedSpeedTemplate?.Type.AsObservable() ?? Observable.Empty<SpeedTemplateType>())
                .Switch()
                .Subscribe(type =>
                    {
                        switch (type)
                        {
                            // Unity ToggleGroup 会自动关掉另一个
                            case SpeedTemplateType.Relative:
                                relativeToggle.isOn = true;
                                break;
                            case SpeedTemplateType.Absolute:
                                absoluteToggle.isOn = true;
                                break;
                        }
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
