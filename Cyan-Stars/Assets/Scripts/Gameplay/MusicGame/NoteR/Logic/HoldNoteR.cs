using CyanStars.Framework;
using CyanStars.Gameplay.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class HoldNoteR : BaseNoteR,INotePos
    {
        public float Pos { get; set; }

        /// <summary>
        /// Hold音符的检查输入结束距离
        /// </summary>
        private float holdCheckInputEndDistance;

        /// <summary>
        /// Hold音符长度
        /// </summary>
        private float holdLength;

        /// <summary>
        /// Hold 音符尾对应的变速组
        /// </summary>
        private SpeedGroup holdEndSpeedGroup;

        /// <summary>
        /// Hole 音符尾与判定线的距离
        /// </summary>
        /// <remarks>提前时为负值</remarks>
        public float EndViewDistance;

        /// <summary>
        /// 是否进行过头判
        /// </summary>
        private bool headChecked;

        /// <summary>
        /// 头判命中时间
        /// </summary>
        private float headCheckTime;

        /// <summary>
        /// 是否有按住的输入
        /// </summary>
        private bool isPressed;

        /// <summary>
        /// 累计有效时长值
        /// </summary>
        private float pressTimeLength;

        /// <summary>
        /// 累计有效时长比例(0-1)
        /// </summary>
        private float value;

        /// <summary>
        /// 结束时间（s）
        /// </summary>
        private float EndTime;

        private const float NoteWidth = 0.2f;

        private readonly MusicGameSettingsModule
            MusicGameSettingsModule = GameRoot.GetDataModule<MusicGameSettingsModule>();

        public override void Init(BaseChartNoteData data, ChartData chartData)
        {
            base.Init(data, chartData);

            Pos = (data as HoldChartNoteData).Pos;

            JudgeTime = chartData.BpmGroups.CalculateTime((data as HoldChartNoteData).JudgeBeat) / 1000f;
            EndTime = chartData.BpmGroups.CalculateTime((data as HoldChartNoteData).EndJudgeBeat) / 1000f;

            holdEndSpeedGroup =
                new SpeedGroup(chartData.SpeedGroups[(data as HoldChartNoteData).HoldEndSpeedGroupIndex],
                    playerSpeed: 1f);

            holdLength = (EndTime - JudgeTime);
            holdCheckInputEndDistance = holdLength; //hold结束时间点与长度相同
        }

        public override bool CanReceiveInput()
        {
            return EvaluateHelper.CheckInputStartDistance <= LogicTimeDistance &&
                   LogicTimeDistance <= holdCheckInputEndDistance;
        }

        public override void OnUpdateInAutoMode(float curLogicTime)
        {
            base.OnUpdateInAutoMode(curLogicTime);

            EndViewDistance = holdEndSpeedGroup.GetDistance(LogicTimeDistance);

            var holdViewObject = ViewObject as HoldViewObject;

            if (!headChecked && LogicTimeDistance <= 0)
            {
                headChecked = true;

                holdViewObject?.OpenFlicker();

                ViewObject.CreateEffectObj(NoteWidth);

                NoteJudgerR.HoldHeadJudge(NoteData as HoldChartNoteData, 0); // Auto Mode 杂率为0

                holdViewObject?.SetPressed(true);
            }

            if (headChecked)
            {
                // 按下时根据已经经过的视图层时间比例计算Hold长度
                float length = Mathf.Abs(ViewDistance - EndViewDistance);
                holdViewObject?.SetLength(length);
            }

            if (LogicTimeDistance < holdCheckInputEndDistance)
            {
                ViewObject?.DestroyEffectObj();
                DestroySelf(false);

                NoteJudgerR.HoldTailJudge(NoteData as HoldChartNoteData, holdLength, 1);
            }
        }

        public override void OnUpdate(float curLogicTime)
        {
            // 1. 判定头判 Miss
            // 2. 累加这一帧的按住时长，并计算视图长度
            // 3. 判定尾判
            float deltaTime = curLogicTime - CurLogicTime;
            base.OnUpdate(curLogicTime);

            EndViewDistance = holdEndSpeedGroup.GetDistance(LogicTimeDistance);

            // 头判 Miss
            if (!headChecked && EvaluateHelper.IsMiss(LogicTimeDistance))
            {
                headChecked = true;
                NoteJudgerR.HoldHeadJudge(NoteData as HoldChartNoteData, LogicTimeDistance);
            }

            // 累加时长
            if (!isPressed)
            {
                //这里isPressed为false 就表示从上一次OnUpdate到这次OnUpdate之间没有Press类型的输入
                ViewObject?.DestroyEffectObj();
            }
            else
            {
                //重置Press标记
                isPressed = false;

                if (-Mathf.Abs(MusicGameSettingsModule.EvaluateRange.Bad) <= LogicTimeDistance &&
                    LogicTimeDistance <= holdCheckInputEndDistance)
                {
                    // 只在 判定时间-Bad区间~结束时间 区间内才累计时长

                    pressTimeLength += deltaTime;

                    // 按下时根据已经经过的视图层时间比例计算 Hold 长度
                    float length = Mathf.Abs(ViewDistance - EndViewDistance);
                    (ViewObject as HoldViewObject)?.SetLength(length);
                }
            }

            // Hold 已结束（当前时间>Hold尾判时间，且当前时间>Hold头判时间+头判Right区间）
            // 判定尾判
            if (holdCheckInputEndDistance < LogicTimeDistance &&
                MusicGameSettingsModule.EvaluateRange.Right < LogicTimeDistance)
            {
                float allLength;
                if (EndTime > JudgeTime + Mathf.Abs(MusicGameSettingsModule.EvaluateRange.Right))
                {
                    // 一般情况：Hold 结束时间大于开始时间+Right区间
                    // 要求按住的总时长s = Hold结束时间 - (Hold开始时间 + Right区间)
                    allLength = EndTime - (JudgeTime + Mathf.Abs(MusicGameSettingsModule.EvaluateRange.Right));
                }
                else
                {
                    // 极短的 Hold：Hold 结束时间小于开始时间+Right区间
                    // 此时只要头判非 Miss，或头判 Miss 但从头判前就按住了对应位置（无KeyDown但KeyPress），尾判都算 Exact
                    allLength = 0;
                }

                if (allLength != 0)
                {
                    // 正常判定
                    pressTimeLength = Mathf.Clamp(pressTimeLength, pressTimeLength, allLength);
                    value = pressTimeLength / allLength;
                    NoteJudgerR.HoldTailJudge(NoteData as HoldChartNoteData, pressTimeLength, value);
                }
                else
                {
                    // 短 Hold 判定
                    if (headCheckTime == 0 && pressTimeLength == 0)
                    {
                        NoteJudgerR.HoldTailJudge(NoteData as HoldChartNoteData, pressTimeLength, 0f);
                    }
                    else
                    {
                        NoteJudgerR.HoldTailJudge(NoteData as HoldChartNoteData, pressTimeLength, 1f);
                    }
                }

                ViewObject?.DestroyEffectObj();
                DestroySelf(false);
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);
            var holdViewObject = ViewObject as HoldViewObject;
            switch (inputType)
            {
                case InputType.Down:
                    if (headChecked)
                    {
                        return;
                    }

                    // 进行头判
                    NoteJudgerR.HoldHeadJudge(NoteData as HoldChartNoteData, LogicTimeDistance);
                    headChecked = true;
                    headCheckTime = CurLogicTime;
                    isPressed = true;

                    // 按键按下，开始截断和特效
                    holdViewObject?.SetPressed(true);
                    ViewObject.CreateEffectObj(NoteWidth);
                    holdViewObject?.OpenFlicker();
                    break;

                case InputType.Press:
                    // 提前按住也计算时长，但不截断或播放特效
                    isPressed = true;

                    if (!headChecked)
                    {
                        return;
                    }

                    holdViewObject?.SetPressed(true);
                    ViewObject.CreateEffectObj(NoteWidth);
                    break;

                case InputType.Up:
                    // 按键抬起，取消截断
                    holdViewObject?.SetPressed(false);
                    break;
            }
        }
    }
}
