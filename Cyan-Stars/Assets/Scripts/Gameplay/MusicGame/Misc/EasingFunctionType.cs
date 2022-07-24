namespace CyanStars.Gameplay.MusicGame
{
    public enum EasingFunctionType //参考效果:http://www.195440.com/html/tween/
    {
        Linear, //线性匀速运动效果
        SineaseIn, //正弦曲线的缓动（sin(t)）/ 从0开始加速的缓动，也就是先慢后快
        SineaseOut, //正弦曲线的缓动（sin(t)）/ 减速到0的缓动，也就是先快后慢
        SineaseInOut, //正弦曲线的缓动（sin(t)）/ 前半段从0开始加速，后半段减速到0的缓动
        BackeaseIn, //超过范围的三次方缓动（(s+1)*t^3 – s*t^2）/ 从0开始加速的缓动，也就是先慢后快
        Custom,
    }
}
