using CyanStars.Framework.Helpers;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Loggers;
using CyanStars.Gameplay.Evaluate;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// Drag音符
    /// </summary>
    public class DragNote : BaseNote
    {
        private bool isHit;

        public override bool CanReceiveInput()
        {
            return LogicTimer <= EvaluateHelper.DragTimeRange && LogicTimer >= -EvaluateHelper.DragTimeRange;
        }

        public override void OnUpdate(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdate(deltaTime, noteSpeedRate);

            if (isHit && LogicTimer <= 0)//接住并过线
            {
                DestroySelf(false);//立即销毁
                return;
            }

            if (LogicTimer < -EvaluateHelper.DragTimeRange)//没接住Miss
            {
                DestroySelf();//延迟销毁

                LogHelper.NoteLogger.Log(new DefaultNoteJudgeLogArgs(data, EvaluateType.Miss));//Log

                GameManager.Instance.maxScore += data.GetFullScore();//更新最理论高分
                GameManager.Instance.RefreshData(-1, -1, EvaluateType.Miss, float.MaxValue);//更新数据
            }
        }

        public override void OnUpdateInAutoMode(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdateInAutoMode(deltaTime, noteSpeedRate);

            if (CanReceiveInput() && !isHit)
            {
                viewObject.CreateEffectObj(data.Width);//生成特效
                
                GameManager.Instance.maxScore += data.GetFullScore();//更新理论最高分
                GameManager.Instance.RefreshData(1, 0.25f, EvaluateType.Exact, 0);//更新数据
                /*
                *Drag的最高分为0.25，所以不能用EvaluateHelper.GetScoreWithEvaluate获取分数
                */
                isHit = true;
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Press) return;//只处理按下的情况

            if (isHit)return;//已经接住了

            viewObject.CreateEffectObj(data.Width);//生成特效
            LogHelper.NoteLogger.Log(new DefaultNoteJudgeLogArgs(data, EvaluateType.Exact));//Log

            GameManager.Instance.maxScore += data.GetFullScore();//更新理论最高分
            GameManager.Instance.RefreshData(1, 0.25f, EvaluateType.Exact, float.MaxValue);//更新数据
            /*
             *Drag的最高分为0.25，所以不能用EvaluateHelper.GetScoreWithEvaluate获取分数
            */

            if (LogicTimer > 0)
            {
                //早按准点放
                isHit = true;
            }
            else
            {
                //晚按即刻放
                DestroySelf(false);
            }
        }
    }
}
