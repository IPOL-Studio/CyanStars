#nullable enable

using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    [RequireComponent(typeof(Canvas))]
    public class PopupView : MonoBehaviour
    {
        private static PopupView? instance;

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


        public void Awake()
        {
            instance = this;
            canvas.enabled = false;
            closeCanvasButton
                .OnClickAsObservable()
                .Subscribe(_ => Close())
                .AddTo(this);
        }

        public static void Show(string title, string describe, bool showCloseButton = false, Dictionary<string, Action?>? buttonCallBackMap = null)
        {
            if (instance.canvas.enabled)
            {
                throw new Exception("已经有一个弹窗打开了，不允许再开一个");
            }

            // 打开弹窗，初始化文本
            instance.titleText.text = title;
            instance.describeText.text = describe;
            instance.closeCanvasButton.gameObject.SetActive(showCloseButton);

            // 防御性删除原有的按钮
            for (int i = instance.buttonsFrame.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(instance.buttonsFrame.transform.GetChild(i).gameObject);
            }

            // 新增按钮并添加回调
            buttonCallBackMap ??= new Dictionary<string, Action?>();
            foreach ((string key, Action? callback) in buttonCallBackMap)
            {
                var go = Instantiate(instance.buttonPrefab, instance.buttonsFrame.transform);
                var popupButton = go.GetComponent<PopupButton>();
                popupButton.Text.text = key;
                popupButton.Button
                    .OnClickAsObservable()
                    .Subscribe(_ =>
                        {
                            callback?.Invoke();
                            Close();
                        }
                    )
                    .AddTo(go);
            }

            instance.canvas.enabled = true;
        }

        public static void Close()
        {
            if (!instance.canvas.enabled)
            {
                throw new Exception("弹窗已经关闭了，不能再次关闭");
            }

            // 关闭弹窗
            instance.canvas.enabled = false;

            // 删除原有的按钮
            for (int i = instance.buttonsFrame.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(instance.buttonsFrame.transform.GetChild(i).gameObject);
            }
        }
    }
}
