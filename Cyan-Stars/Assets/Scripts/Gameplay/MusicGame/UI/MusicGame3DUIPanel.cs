using System;
using System.Collections;
using UnityEngine;
using TMPro;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游3DUI界面
    /// </summary>
    [UIData(UIGroupName = UIConst.UIGroup3D,
        UIPrefabName = "Assets/BundleRes/Prefabs/MusicGameUI/MusicGame3DUIPanel.prefab")]
    public class MusicGame3DUIPanel : BaseUIPanel
    {
        [SerializeField]
        private TextMeshProUGUI txtGrade;

        private MusicGamePlayingDataModule playingDataModule;

        protected override void OnCreate()
        {
            playingDataModule = GameRoot.GetDataModule<MusicGamePlayingDataModule>();
        }


        public override void OnOpen()
        {
            txtGrade.text = "Ready?";
            Color color = txtGrade.color;
            color.a = 1;
            txtGrade.color = color;

            GameRoot.Event.AddListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
        }

        public override void OnClose()
        {
            GameRoot.Event.RemoveListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
        }

        /// <summary>
        /// 音游数据刷新回调
        /// </summary>
        private void OnMusicGameDataRefresh(object sender, EventArgs args)
        {
            //刷新评级
            Color color = Color.white;
            if (playingDataModule.IsAutoMode)
            {
                txtGrade.text = "AUTO";
                color = Color.green;
            }
            else
            {
                txtGrade.text = playingDataModule.MusicGamePlayData.Grade.ToString();

                switch (playingDataModule.MusicGamePlayData.Grade)
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
            txtGrade.color = color;
            txtGrade.fontSize = 12;
            StopAllCoroutines();
            StartCoroutine(FadeGradeTMP());

            // //刷新杂率
            // TxtImpurityRate.text = $"杂率:{playingDataModule.MusicGamePlayData.ImpurityRate:F3}ms";
            //
            // if (playingDataModule.MusicGamePlayData.ImpurityRate < 30)
            // {
            //     TxtImpurityRate.color = Color.yellow;
            // }
            // else if (playingDataModule.MusicGamePlayData.ImpurityRate < 50)
            // {
            //     TxtImpurityRate.color = Color.blue;
            // }
            // else
            // {
            //     TxtImpurityRate.color = Color.white;
            // }
            //
            // //刷新得分率
            // float scoreRatio = 0;
            // if (playingDataModule.MusicGamePlayData.MaxScore > 0)
            // {
            //     scoreRatio = playingDataModule.MusicGamePlayData.Score / playingDataModule.MusicGamePlayData.MaxScore;
            // }
            //
            // TxtScoreRatio.text = $"{(scoreRatio * 100):F}%";
            //
            // if (playingDataModule.MusicGamePlayData.GreatNum + playingDataModule.MusicGamePlayData.RightNum + playingDataModule.MusicGamePlayData.OutNum + playingDataModule.MusicGamePlayData.BadNum +
            //     playingDataModule.MusicGamePlayData.MissNum == 0)
            // {
            //     TxtScoreRatio.color = Color.yellow;
            // }
            // else
            // {
            //     if (playingDataModule.MusicGamePlayData.MissNum + playingDataModule.MusicGamePlayData.BadNum == 0)
            //     {
            //         TxtScoreRatio.color = Color.cyan;
            //     }
            //     else
            //     {
            //         TxtScoreRatio.color = Color.white;
            //     }
            // }
            //
            // //刷新当前分数
            // TxtVisibleScore.text = ((int)(playingDataModule.MusicGamePlayData.Score / playingDataModule.MusicGamePlayData.FullScore * 100000)).ToString().PadLeft(6, '0'); //更新文本
        }

        private IEnumerator FadeGradeTMP()
        {
            yield return new WaitForSeconds(0.1f);

            Color gradeColor = txtGrade.color;
            txtGrade.fontSize = 11;

            float a = 1;
            while (a >= 0)
            {
                a -= Time.deltaTime;
                gradeColor.a = a;
                txtGrade.color = gradeColor;
                yield return null;
            }
        }
    }
}
