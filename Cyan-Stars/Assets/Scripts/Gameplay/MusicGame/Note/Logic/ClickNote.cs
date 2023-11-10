using CyanStars.Framework;
using CyanStars.Framework.Logging;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// Click音符
    /// </summary>
    public class ClickNote : BaseNote
    {
        /// <summary>
        /// 按下的时间点
        /// </summary>
        private float downTimePoint;

        /// <summary>
        /// 是否进行过头判
        /// </summary>
        private bool headChecked;

        public override void OnUpdate(float curLogicTime,float curViewTime)
        {
            base.OnUpdate(curLogicTime, curViewTime);

            if (EvaluateHelper.IsMiss(Distance))
            {
                if (!headChecked)
                {
                    //没接住 miss
                    DestroySelf();

                    NoteJudger.ClickMiss(Data);
                }
                else
                {
                    //头判成功过且超时未抬起 结算尾判
                    ViewObject.CreateEffectObj(NoteData.NoteWidth);
                    DestroySelf(false);

                    float timeLength = curLogicTime - downTimePoint;
                    NoteJudger.ClickTailJudge(Data,timeLength);
                }

            }


        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (EvaluateHelper.GetTapEvaluate(Distance) == EvaluateType.Exact && !headChecked)
            {
                headChecked = true;
                DataModule.MaxScore += 2;
                DataModule.RefreshPlayingData(1, 2, EvaluateType.Exact, 0); // Auto Mode 杂率为0

                NoteJudger.LogJudgedInfo(new ClickNoteJudgedInfo(Data, EvaluateType.Exact, 0));

                ViewObject.CreateEffectObj(NoteData.NoteWidth);
                DestroySelf(false);
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            switch (inputType)
            {
                case InputType.Down:

                    downTimePoint = CurLogicTime;

                    if (!headChecked)
                    {
                        headChecked = true;

                        //处理头判
                        EvaluateType et =  NoteJudger.ClickHeadJudge(Data, Distance);
                        if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                        {
                            //头判失败直接销毁
                            ViewObject.CreateEffectObj(NoteData.NoteWidth);
                            DestroySelf(false);
                        }
                    }

                    break;
                case InputType.Up:

                    if (!headChecked) return;

                    ViewObject.CreateEffectObj(NoteData.NoteWidth);
                    DestroySelf(false);

                    float timeLength = CurLogicTime - downTimePoint;
                    NoteJudger.ClickTailJudge(Data,timeLength);

                    break;
            }
        }


    }
}
