using System;
using CyanStars.Framework;

namespace CyanStars.Gameplay.MusicGame
{
    //TODO: 实现SettingsModule后，将此实现移入其中
    public class EvaluateModule : BaseDataModule
    {
        private EvaluateRange normalRange;
        private EvaluateRange hardRange;
        private EvaluateRange easyRange;

        public EvaluateRange Current { get; private set; }

        public override void OnInit()
        {
            normalRange = new EvaluateRange(0.08f, 0.14f, 0.2f, -0.23f);
            hardRange = new EvaluateRange(0.04f, 0.1f, 0.2f, -0.16f);
            easyRange = new EvaluateRange(0.12f, 0.18f, 0.2f, -0.23f);

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
