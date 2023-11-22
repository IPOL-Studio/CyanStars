using CyanStars.Framework.Logging;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// Drag音符
    /// </summary>
    public class DragNote : BaseNote
    {
        private bool isHit;

        public override bool CanReceiveInput()
        {
            return Distance <= EvaluateHelper.DragJudgeDistanceRange && Distance >= -EvaluateHelper.DragJudgeDistanceRange;
        }

        public override void OnUpdate(float curLogicTime,float curViewTime)
        {
            base.OnUpdate(curLogicTime,curViewTime);

            if (isHit && Distance <= 0) //接住并过线
            {
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //立即销毁
                return;
            }

            if (Distance < -EvaluateHelper.DragJudgeDistanceRange) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                NoteJudger.DragJudge(Data,true);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime,curViewTime);

            if (Distance <= 0)
            {
                isHit = true;

                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //立即销毁

                NoteJudger.DragJudge(Data,false);
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Press) return; //只处理按下的情况
            if (isHit) return; //已经接住了

            if (Distance > 0)
            {
                //早按准点放
                isHit = true;
            }
            else
            {
                //晚按即刻放
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false);
            }

            NoteJudger.DragJudge(Data,false);
        }
    }
}
