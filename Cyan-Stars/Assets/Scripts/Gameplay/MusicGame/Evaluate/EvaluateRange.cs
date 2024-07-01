namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 判定时间范围
    /// <para>Exact 与 Great仅考虑绝对值情况</para>
    /// <para>判定数值均包含填入数值</para>
    /// <para>相关规范: https://github.com/IPOL-Studio/CyanStars/issues/80 </para>
    /// </summary>
    public class EvaluateRange
    {
        public readonly float Exact;
        public readonly float Great;
        public readonly float Bad;
        public readonly float Right;

        public EvaluateRange(float exact, float great, float bad, float right)
        {
            Exact = exact;
            Great = great;
            Bad = bad;
            Right = right;
        }

        public override string ToString()
        {
            return $"Exact ±{Exact}, Great ±{Great}, Bad {Bad}, Right {Right}";
        }
    }
}
