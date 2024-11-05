using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using TMPro;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/ScoreSettlementPanel.prefab")]
    public class ScoreSettlementPanel : BaseUIPanel
    {
        // 硬编码渐变时间配置
        private const float ScoreNumRiseTime = 2.0f;
        private const float ImpurityRateNumRiseTime = 1.0f;
        private const float MaxComboNumRiseTime = 1.5f;

        // Unity组件
        public TextMeshProUGUI TextScoreNum;
        public TextMeshProUGUI TextImpurityRateNum;
        public TextMeshProUGUI TextMaxComboNum;

        // 由音游程序传入目标数据
        public int TargetScoreNum { get; set; }
        public float TargetImpurityRateNum { get; set; }
        public int TargetMaxComboNum { get; set; }

        // 内部变量
        private float StartTime { get; set; }


        public override void OnOpen()
        {
            StartTime = Time.time;
            TargetScoreNum = 1000000; // TODO: 测试用，实装时记得换成游戏结束时实际数据
            TargetImpurityRateNum = 20.4f;
            TargetMaxComboNum = 4000;
        }

        public void Update()
        {
            if (Time.time <= (StartTime + ScoreNumRiseTime))
            {
                TextScoreNum.text = UpdateScoreNumDisplay();
            }

            if (Time.time <= (StartTime + ImpurityRateNumRiseTime))
            {
                TextImpurityRateNum.text = UpdateImpurityRateNumDisplay();
            }

            if (Time.time <= (StartTime + MaxComboNumRiseTime))
            {
                TextMaxComboNum.text = UpdateMaxComboNumDisplay();
            }
        }

        /// <summary>
        /// 获取实时分数文本
        /// </summary>
        /// <returns>实时分数</returns>
        private string UpdateScoreNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    TargetScoreNum,
                    Mathf.Min(Time.time - StartTime, ScoreNumRiseTime),
                    ScoreNumRiseTime)
            ).ToString("D7"); // 格式化为7位数字，不足的位数用"0"填充
        }

        /// <summary>
        /// 获取实时杂率文本
        /// </summary>
        /// <returns>实时杂率</returns>
        private string UpdateImpurityRateNumDisplay()
        {
            return (Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    100.0f,
                    TargetImpurityRateNum,
                    Mathf.Min(Time.time - StartTime, ImpurityRateNumRiseTime),
                    ImpurityRateNumRiseTime) * 10) / 10f).ToString("00.0");
        }

        /// <summary>
        /// 获取实时MaxCombo文本
        /// </summary>
        /// <returns>实时MaxCombo</returns>
        private string UpdateMaxComboNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    TargetMaxComboNum,
                    Mathf.Min(Time.time - StartTime, MaxComboNumRiseTime),
                    MaxComboNumRiseTime)
            ).ToString(); // 格式化为7位数字，不足的位数用"0"填充
        }
    }
}
