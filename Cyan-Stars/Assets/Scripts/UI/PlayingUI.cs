using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayingUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI comboText;//combo文本组件
    public TMPro.TextMeshProUGUI scoreText;//score文本组件
    void Update()
    {
        comboText.text = "COMBO:" + GameManager.Instance.combo;//更新文本
        scoreText.text = "SCORE:" + GameManager.Instance.score;//更新文本
    }
}
//This code is writed by Ybr.