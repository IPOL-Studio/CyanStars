namespace CyanStars.Framework.FSM
{
    /// <summary>
    /// 有限状态机状态基类
    /// </summary>
    public abstract class BaseState
    {
        /// <summary>
        /// 持有此状态的有限状态机
        /// </summary>
        protected FSM owner;


        /// <summary>
        /// 设置持有此状态的有限状态机
        /// </summary>
        public void SetOwner(FSM owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// 进入状态
        /// </summary>
        public abstract void OnEnter();

        /// <summary>
        /// 轮询状态
        /// </summary>
        public abstract void OnUpdate(float deltaTime);

        /// <summary>
        /// 退出状态
        /// </summary>
        public abstract void OnExit();
    }
}
