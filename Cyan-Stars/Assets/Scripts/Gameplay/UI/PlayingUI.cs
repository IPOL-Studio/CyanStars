using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CyanStars.Framework;

namespace CyanStars.Gameplay.UI
{
    public class PlayingUI : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI comboText; //combo文本组件
        public TMPro.TextMeshProUGUI scoreText; //score文本组件
        public TMPro.TextMeshProUGUI visibleScoreText; //显示的score文本组件
        public TMPro.TextMeshProUGUI gradeText; //grade文本组件
        public TMPro.TextMeshProUGUI currentDeviationText; //currentDeviation文本组件
        public TMPro.TextMeshProUGUI accuracyText; //accuracy文本组件
        public TMPro.TextMeshProUGUI scoreRatioText; //scoreRatio文本组件
        public TMPro.TextMeshProUGUI LrcText; //歌词文本组件
        public bool showDeviation; //是否显示杂率
        public Image img;

        public void Refresh(int combo, float score, string grade, float currentDeviation)
        {
            if (comboText) comboText.text = combo.ToString(); //更新文本
            if (scoreText) scoreText.text = "SCORE(DEBUG):" + score; //更新文本
            if (visibleScoreText)
                visibleScoreText.text =
                    ((int)(score /  GameRoot.GetDataModule<MusicGameModule>().FullScore * 100000)).ToString().PadLeft(6, '0'); //更新文本
            if (gradeText)
            {
                if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode)
                {
                    gradeText.text = "AUTO";

                }
                else
                {
                    gradeText.text = grade;
                }
            }

            if (gradeText)
            {
                Color color = Color.white;
                if (grade == "Exact" || grade == "Auto") color = Color.green;
                if (grade == "Great") color = Color.cyan;
                if (grade == "Right" || grade == "Out") color = Color.yellow;
                if (grade == "Bad") color = Color.red;
                if (grade == "Miss") color = Color.white;
                color.a = 1;
                gradeText.color = color;
                gradeText.fontSize = 12;
                StopAllCoroutines();
                StartCoroutine(FadeGradeTMP());
            }
        }

        IEnumerator FadeGradeTMP()
        {
            Color GradeColor;
            if (gradeText) GradeColor = gradeText.color;
            else GradeColor = Color.black;
            yield return new WaitForSeconds(0.1f);
            if(gradeText) gradeText.fontSize = 11;
            for (float i = 1; i >= 0; i -= 0.01f)
            {
                GradeColor.a = i;
                if (gradeText) gradeText.color = GradeColor;
                yield return new WaitForSeconds(0.01f);
            }
        }

        void Update()
        {
            if (img) img.fillAmount = GameRoot.GetDataModule<MusicGameModule>().TimeSchedule();
            if (showDeviation)
            {
                if (currentDeviationText)
                {
                    if (GameRoot.GetDataModule<MusicGameModule>().CurrentDeviation > 0)
                    {
                        currentDeviationText.text = "+" + string.Format("{0:F3}",
                                                        GameRoot.GetDataModule<MusicGameModule>().CurrentDeviation * 1000) + "ms";
                        currentDeviationText.color = Color.red / 1.35f;
                    }

                    if (GameRoot.GetDataModule<MusicGameModule>().CurrentDeviation < 0)
                    {
                        currentDeviationText.text = string.Format("{0:F3}",
                                                        GameRoot.GetDataModule<MusicGameModule>().CurrentDeviation * 1000) + "ms";
                        currentDeviationText.color = Color.cyan / 1.35f;
                    }
                }

                if (accuracyText)
                {
                    float accuracy = 0, sum = 0;
                    if (GameRoot.GetDataModule<MusicGameModule>().DeviationList.Count > 0)
                    {
                        foreach (var item in GameRoot.GetDataModule<MusicGameModule>().DeviationList)
                        {
                            sum += Mathf.Abs(item);
                        }

                        accuracy = sum / (float)GameRoot.GetDataModule<MusicGameModule>().DeviationList.Count;
                    }

                    accuracyText.text = "杂率:" + string.Format("{0:F3}", accuracy) + "s";
                    if (accuracy < 0.03) accuracyText.color = Color.yellow;
                    else if (accuracy < 0.05) accuracyText.color = Color.blue;
                    else accuracyText.color = Color.white;
                }
            }
            else
            {
                if (currentDeviationText) currentDeviationText.text = "";
                if (accuracyText) accuracyText.text = "";
            }

            if (scoreRatioText)
            {
                float scoreRatio = 0;
                if (GameRoot.GetDataModule<MusicGameModule>().MaxScore > 0)
                {
                    scoreRatio = (float)GameRoot.GetDataModule<MusicGameModule>().Score / (float)GameRoot.GetDataModule<MusicGameModule>().MaxScore;
                }

                scoreRatioText.text = string.Format("{0:F}", scoreRatio * 100) + "%";
                if (GameRoot.GetDataModule<MusicGameModule>().GreatNum + GameRoot.GetDataModule<MusicGameModule>().RightNum + GameRoot.GetDataModule<MusicGameModule>().BadNum +
                    GameRoot.GetDataModule<MusicGameModule>().MissNum == 0)
                {
                    scoreRatioText.color = Color.yellow;
                }
                else
                {
                    if (GameRoot.GetDataModule<MusicGameModule>().MissNum + GameRoot.GetDataModule<MusicGameModule>().BadNum == 0)
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
                LrcText.text = GameRoot.GetDataModule<MusicGameModule>().CurLrcText;
            }
        }
    }
}
