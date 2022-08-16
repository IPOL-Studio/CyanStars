using CyanStars.Framework;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 输入接收器基类
    /// </summary>
    public abstract class BaseInputReceiver
    {
        /// <summary>
        /// 输入映射数据
        /// </summary>
        protected readonly InputMapData InputMapData;

        public BaseInputReceiver(InputMapData data)
        {
            InputMapData = data;
        }

        /// <summary>
        /// 开始接收输入
        /// </summary>
        public abstract void StartReceive();

        /// <summary>
        /// 结束接收输入
        /// </summary>
        public abstract void EndReceive();


    }
}
