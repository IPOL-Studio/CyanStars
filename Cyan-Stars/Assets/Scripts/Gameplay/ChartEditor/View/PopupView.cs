#nullable enable

using System;
using CyanStars.GamePlay.ChartEditor;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    [RequireComponent(typeof(Canvas))]
    public class PopupView : BaseView<PopupViewModel>
    {
        [SerializeField]
        private Canvas canvas = null!;

        [SerializeField]
        private GameObject buttonPrefab = null!;

        [SerializeField]
        private Button closeCanvasButton = null!;

        [SerializeField]
        private TMP_Text titleText = null!;

        [SerializeField]
        private TMP_Text describeText = null!;

        [SerializeField]
        private GameObject buttonsFrame = null!;


        public override void Bind(PopupViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            closeCanvasButton.onClick.AddListener(() => ViewModel.ClosePopup());

            ViewModel.CanvasVisibility
                .Subscribe(isOn =>
                    {
                        if (isOn)
                        {
                            // 打开弹窗，初始化文本
                            closeCanvasButton.gameObject.SetActive(ViewModel.ShowCloseButton);
                            titleText.text = ViewModel.TitleString;
                            describeText.text = ViewModel.DescribeString;

                            // 防御性删除原有的按钮
                            for (int i = buttonsFrame.transform.childCount - 1; i >= 0; i--)
                            {
                                Destroy(buttonsFrame.transform.GetChild(i).gameObject);
                            }

                            // 新增按钮并添加回调
                            foreach ((string key, Action? callback) in ViewModel.ButtonCallBackMap)
                            {
                                var go = Instantiate(buttonPrefab, canvas.transform);
                                var popupButton = go.GetComponent<PopupButton>();
                                popupButton.Text.text = key;
                                popupButton.Button.onClick.AddListener(() =>
                                    {
                                        callback?.Invoke();
                                        ViewModel.ClosePopup();
                                    }
                                );
                            }

                            canvas.enabled = true;
                        }
                        else
                        {
                            // 关闭弹窗
                            canvas.enabled = false;

                            // 删除原有的按钮
                            for (int i = buttonsFrame.transform.childCount - 1; i >= 0; i--)
                            {
                                Destroy(buttonsFrame.transform.GetChild(i).gameObject);
                            }
                        }
                    }
                )
                .AddTo(this);
        }

        protected override void OnDestroy()
        {
            closeCanvasButton.onClick.RemoveAllListeners();
        }
    }
}
