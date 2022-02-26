using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayingUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI comboText;//combo文本组件
    public TMPro.TextMeshProUGUI scoreText;//score文本组件
    public TMPro.TextMeshProUGUI gradeText;//grade文本组件
    public TMPro.TextMeshProUGUI currentDeviationText;//currentDeviation文本组件
    public TMPro.TextMeshProUGUI accuracyText;//accuracy文本组件
    public TMPro.TextMeshProUGUI scoreRatioText;//scoreRatio文本组件
    public bool showDeviation;//是否显示杂率
    public Image img;
    public void Refresh(int combo,int score,string grade,float currentDeviation)
    {
        if(comboText)comboText.text = "COMBO:" + combo;//更新文本
        if(scoreText)scoreText.text = "SCORE:" + score;//更新文本
        if(gradeText)gradeText.text = grade;//更新文本
        if(gradeText)
        {
            if(grade == "Exact")gradeText.color = Color.green;
            if(grade == "Great")gradeText.color = Color.cyan;
            if(grade == "Right")gradeText.color = Color.yellow;
            if(grade == "Bad")gradeText.color = Color.red;
            if(grade == "Miss")gradeText.color = Color.white;
        }
    }
    void Update()
    {
        //if(img)img.fillAmount = GameMgr.Instance.TimeSchedule();
        if(showDeviation)
        {
            if(currentDeviationText)
            {
                currentDeviationText.text = "当前杂率:" + string.Format("{0:F}",GameManager.Instance.currentDeviation) + "ms";
                if(GameManager.Instance.currentDeviation > 0)currentDeviationText.color = Color.red;
                if(GameManager.Instance.currentDeviation < 0)currentDeviationText.color = Color.cyan;
            }
            if(accuracyText)
            {
                float accuracy = 0,sum = 0;
                if(GameManager.Instance.deviationList.Count > 0)
                {
                    foreach(var item in GameManager.Instance.deviationList)
                    {
                        sum += item;
                    }
                    accuracy = sum / (float)GameManager.Instance.deviationList.Count;
                }
                accuracyText.text = "平均杂率:" + string.Format("{0:F}",accuracy) + "ms";
                if(accuracy > 0)accuracyText.color = Color.red;
                if(accuracy < 0)accuracyText.color = Color.cyan;
            }
        }
        else
        {
            if(currentDeviationText)currentDeviationText.text = "";
            if(accuracyText)accuracyText.text = "";
        }
        if(scoreRatioText)
        {
            float scoreRatio = 0;
            if(GameManager.Instance.maxScore > 0)
            {
                scoreRatio = (float)GameManager.Instance.score / (float)GameManager.Instance.maxScore;
            }
            scoreRatioText.text = "得分比:" + string.Format("{0:F}",scoreRatio*100) + "%";
            if(GameManager.Instance.greatNum + GameManager.Instance.rightNum + GameManager.Instance.badNum + GameManager.Instance.missNum == 0)
            {
                scoreRatioText.color = Color.yellow;
            }
            else
            {
                if(GameManager.Instance.missNum + GameManager.Instance.badNum == 0)
                {
                    scoreRatioText.color = Color.cyan;
                }
                else
                {
                    scoreRatioText.color = Color.white;
                }
            }
        }
    }
}
//This code is writed by Ybr.