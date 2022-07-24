using System;
using System.Collections;
using UnityEngine;
using TMPro;
using CyanStars.Framework;
using CyanStars.Framework.UI;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游3DUI界面
    /// </summary>
    [UIData(UIGroupName = UIConst.UIGroup3D,
        UIPrefabName = "Assets/BundleRes/Prefabs/MusicGameUI/MusicGame3DUIPanel.prefab")]
    public class MusicGame3DUIPanel : BaseUIPanel
    {
        public TextMeshProUGUI TxtGrade;
        public TextMeshProUGUI TxtAccuracy;
        public TextMeshProUGUI TxtScoreRatio;
        public TextMeshProUGUI TxtVisibleScore;

        private MusicGameModule dataModule;

        protected override void OnCreate()
        {
            dataModule = GameRoot.GetDataModule<MusicGameModule>();
        }


        public override void OnOpen()
        {
            GameRoot.Event.AddListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
        }

        public override void OnClose()
        {
            TxtGrade.text = null;
            TxtAccuracy.text = $"杂率:{0:F3}s";
            TxtAccuracy.color = Color.yellow;
            TxtScoreRatio.text = $"{0:F}%";
            TxtScoreRatio.color = Color.cyan;
            TxtVisibleScore.text = null;

            GameRoot.Event.RemoveListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
        }

        /// <summary>
        /// 音游数据刷新回调
        /// </summary>
        private void OnMusicGameDataRefresh(object sender, EventArgs args)
        {
            //刷新评级
            Color color = Color.white;
            if (dataModule.IsAutoMode)
            {
                TxtGrade.text = "AUTO";
                color = Color.green;
            }
            else
            {
                TxtGrade.text = dataModule.Grade.ToString();

                switch (dataModule.Grade)
                {
                    case EvaluateType.Miss:
                        color = Color.white;
                        break;
                    case EvaluateType.Exact:
                        color = Color.green;
                        break;
                    case EvaluateType.Great:
                        color = Color.cyan;
                        break;
                    case EvaluateType.Bad:
                        color = Color.red;
                        break;
                    case EvaluateType.Right:
                    case EvaluateType.Out:
                        color = Color.yellow;
                        break;
                }
            }

            color.a = 1;
            TxtGrade.color = color;
            TxtGrade.fontSize = 12;
            StopAllCoroutines();
            StartCoroutine(FadeGradeTMP());

            //刷新杂率
            float accuracy = 0, sum = 0;
            if (dataModule.DeviationList.Count > 0)
            {
                foreach (var item in dataModule.DeviationList)
                {
                    sum += Mathf.Abs(item);
                }

                accuracy = sum / dataModule.DeviationList.Count;
            }

            TxtAccuracy.text = $"杂率:{accuracy:F3}s";

            if (accuracy < 0.03)
            {
                TxtAccuracy.color = Color.yellow;
            }
            else if (accuracy < 0.05)
            {
                TxtAccuracy.color = Color.blue;
            }
            else
            {
                TxtAccuracy.color = Color.white;
            }

            //刷新得分率
            float scoreRatio = 0;
            if (dataModule.MaxScore > 0)
            {
                scoreRatio = dataModule.Score / dataModule.MaxScore;
            }

            TxtScoreRatio.text = $"{(scoreRatio * 100):F}%";

            if (dataModule.GreatNum + dataModule.RightNum + dataModule.BadNum +
                dataModule.MissNum == 0)
            {
                TxtScoreRatio.color = Color.yellow;
            }
            else
            {
                if (dataModule.MissNum + dataModule.BadNum == 0)
                {
                    TxtScoreRatio.color = Color.cyan;
                }
                else
                {
                    TxtScoreRatio.color = Color.white;
                }
            }

            //刷新当前分数
            TxtVisibleScore.text = ((int)(dataModule.Score / dataModule.FullScore * 100000)).ToString().PadLeft(6, '0'); //更新文本
        }

        private IEnumerator FadeGradeTMP()
        {
            yield return new WaitForSeconds(0.1f);

            Color gradeColor = TxtGrade.color;
            TxtGrade.fontSize = 11;

            float a = 1;
            while (a >= 0)
            {
                a -= Time.deltaTime;
                gradeColor.a = a;
                TxtGrade.color = gradeColor;
                yield return null;
            }
        }
    }
}
