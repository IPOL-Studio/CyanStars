#nullable enable

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using CyanStars.GamePlay.ChartEditor.Model;
using CyanStars.GamePlay.ChartEditor.View;


[RequireComponent(typeof(Canvas))]
public class PopupCanvas : BaseView
{
    [Header("UI 引用")]
    [SerializeField]
    private Canvas canvas = null!;

    [SerializeField]
    private GameObject closeButton = null!; // 叉号按钮

    [SerializeField]
    private TextMeshProUGUI titleText = null!; // 标题文本

    [SerializeField]
    private TextMeshProUGUI messageText = null!; // 说明文本

    [Header("按钮模板和容器")]
    [SerializeField]
    private GameObject buttonPrefab = null!; // 单个按钮的预制体

    [SerializeField]
    private Transform buttonContainer = null!; // 存放按钮的容器

    // 存储当前弹窗的按钮，方便清理
    private readonly List<GameObject> CurrentButtons = new List<GameObject>();

    public override void Bind(ChartEditorModel chartEditorModel)
    {
        base.Bind(chartEditorModel);

        canvas.enabled = false;
        if (buttonPrefab != null)
        {
            buttonPrefab.SetActive(false);
        }

        Model.OnShowPopupRequest += HandleShowPopupRequest;
    }

    private void HandleShowPopupRequest(PopupData data)
    {
        Show(data.Title, data.Message, data.ShowCloseButton, data.Buttons);
    }

    /// <summary>
    /// 初始化并显示弹窗
    /// </summary>
    /// <param name="title">弹窗标题</param>
    /// <param name="message">弹窗说明</param>
    /// <param name="showCloseButton">是否显示右上角的叉号</param>
    /// <param name="buttons">按钮数据列表</param>
    private void Show(string title,
        string message,
        bool showCloseButton = true,
        params KeyValuePair<string, Action?>[]? buttons)
    {
        // 设置基础信息
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (closeButton != null)
        {
            closeButton.SetActive(showCloseButton);
            closeButton.GetComponent<Button>()?.onClick.AddListener(Close);
        }

        // 清理旧按钮
        ClearButtons();

        // 根据数据创建新按钮
        if (buttons != null && buttonPrefab != null && buttonContainer != null)
        {
            foreach (var buttonData in buttons)
            {
                GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
                CurrentButtons.Add(buttonGO);

                Button button = buttonGO.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    buttonText.text = buttonData.Key;
                }

                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() =>
                    {
                        buttonData.Value?.Invoke();
                        Close();
                    });
                }

                buttonGO.SetActive(true);
            }
        }

        canvas.enabled = true;
    }

    /// <summary>
    /// 关闭弹窗
    /// </summary>
    private void Close()
    {
        canvas.enabled = false;
    }

    /// <summary>
    /// 清理所有动态生成的按钮
    /// </summary>
    private void ClearButtons()
    {
        foreach (var button in CurrentButtons)
        {
            Destroy(button);
        }

        CurrentButtons.Clear();
    }

    private void OnDestroy()
    {
        Model.OnShowPopupRequest -= HandleShowPopupRequest;
    }
}
