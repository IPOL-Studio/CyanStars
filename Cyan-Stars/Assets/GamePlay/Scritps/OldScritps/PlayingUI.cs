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
    public TMPro.TextMeshProUGUI LrcText;  //歌词文本组件
    public bool showDeviation;//是否显示杂率
    public Image img;
    public void Refresh(int combo,float score,string grade,float currentDeviation)
    {
        if(comboText)comboText.text = "COMBO:" + combo;//更新文本
        if(scoreText)scoreText.text = "SCORE:" + score;//更新文本
        if(gradeText)
        {
            if(GameManager.Instance.isAutoMode)
            {
                gradeText.text = "AUTO";
                
            }
            else
            {
                gradeText.text = grade;
            }
        }
        if(gradeText)
        {
            Color color = Color.white;
            if(grade == "Exact" || grade == "Auto")color = Color.green;
            if(grade == "Great")color = Color.cyan;
            if(grade == "Right" || grade =="Out")color = Color.yellow;
            if(grade == "Bad")color = Color.red;
            if(grade == "Miss")color = Color.white;
            color.a = 1;
            gradeText.color = color;
            gradeText.fontSize = 12;
            StopAllCoroutines();
            StartCoroutine(FadeGradeTMP());
        }
    }
    IEnumerator FadeGradeTMP()
    {
        Color color = gradeText.color;
        yield return new WaitForSeconds(0.1f);
        gradeText.fontSize = 11;
        for(float i = 1;i >= 0;i -= 0.01f)
        {
            color.a = i;
            gradeText.color = color;
            yield return new WaitForSeconds(0.01f);
        }
    }
    void Update()
    {
        if(img)img.fillAmount = GameMgr.Instance.TimeSchedule();
        if(showDeviation)
        {
            if(currentDeviationText)
            {
                currentDeviationText.text = "误差:" + string.Format("{0:F3}",GameManager.Instance.currentDeviation*1000) + "ms";
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
                        sum += Mathf.Abs(item);
                    }
                    accuracy = sum / (float)GameManager.Instance.deviationList.Count;
                }
                accuracyText.text = "杂率:" + string.Format("{0:F3}",accuracy) + "s";
                if(accuracy < 0.03)accuracyText.color = Color.yellow;
                else if(accuracy < 0.05)accuracyText.color = Color.blue;
                else accuracyText.color = Color.white;
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
            scoreRatioText.text = "得分率:" + string.Format("{0:F}",scoreRatio*100) + "%";
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

        if (LrcText)
        {
            LrcText.text = GameManager.Instance.curLrcText;
        }
    }
}
//This code is writed by Ybr.