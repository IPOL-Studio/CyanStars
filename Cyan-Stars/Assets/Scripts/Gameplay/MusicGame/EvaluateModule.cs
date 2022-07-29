using System;
using CyanStars.Framework;

namespace CyanStars.Gameplay.MusicGame
{
    public class EvaluateModule : BaseDataModule
    {
        private EvaluateRange normalRange;
        private EvaluateRange hardRange;
        private EvaluateRange easyRange;

        public EvaluateRange Current { get; private set; }

        public override void OnInit()
        {
            normalRange = new EvaluateRange(0.08f, 0.014f, 0.2f, -0.23f, -0.231f);
            hardRange = new EvaluateRange(0.04f, 0.1f, 0.2f, -0.16f, -0.161f);
            easyRange = new EvaluateRange(0.12f, 0.18f, 0.2f, -0.23f, -0.231f);

            Current = normalRange;
        }

        public void ChangeMode(EvaluateMode mode)
        {
            Current = mode switch
            {
                EvaluateMode.Easy => easyRange,
                EvaluateMode.Normal => normalRange,
                EvaluateMode.Hard => hardRange,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
