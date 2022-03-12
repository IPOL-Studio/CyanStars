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
    public void Refresh(int combo, float score, EvaluateType grade, float currentDeviation)
    {
        if(comboText)comboText.text = "COMBO:" + combo;//更新文本
        if(scoreText)scoreText.text = "SCORE:" + score;//更新文本
        if(gradeText)
        {
            gradeText.text = grade.ToString();//更新文本
            gradeText.color = grade switch
            {
                EvaluateType.Exact => Color.green,
                EvaluateType.Great => Color.cyan,
                EvaluateType.Right => Color.yellow,
                EvaluateType.Out   => Color.yellow,
                EvaluateType.Bad   => Color.red,
                _                  => Color.white
            };
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
                var currentDeviation = GameManager.Instance.currentDeviation;
                currentDeviationText.text = $"误差:{currentDeviation * 1000:F3}ms";
                currentDeviationText.color = currentDeviation > 0 ? Color.red : Color.cyan;
            }
            
            if(accuracyText)
            {
                float accuracy = 0, sum = 0;
                if(GameManager.Instance.deviationList.Count > 0)
                {
                    foreach(var item in GameManager.Instance.deviationList)
                    {
                        sum += Mathf.Abs(item);
                    }
                    accuracy = sum / GameManager.Instance.deviationList.Count;
                }
                accuracyText.text = $"杂率:{accuracy:F3}s";
                
                if(accuracy < 0.03) accuracyText.color = Color.yellow;
                else if(accuracy < 0.05) accuracyText.color = Color.blue;
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
                scoreRatio = GameManager.Instance.score / GameManager.Instance.maxScore;
            }
            scoreRatioText.text = $"得分率:{scoreRatio * 100:F}%";
            if(GameManager.Instance.greatNum + GameManager.Instance.rightNum + GameManager.Instance.badNum + GameManager.Instance.missNum == 0)
            {
                scoreRatioText.color = Color.yellow;
            }
            else
            {
                scoreRatioText.color = GameManager.Instance.missNum + GameManager.Instance.badNum == 0 ? Color.cyan : Color.white;
            }
        }
    }
}
//This code is writed by Ybr.