using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    private TMPro.TextMeshProUGUI text;//文本组件
    void Start()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();//获取文本组件
    }
    void Update()
    {
        text.text = "COMBO:" + GameManager.Instance.combo;//更新文本
    }
}
//This code is writed by Ybr.