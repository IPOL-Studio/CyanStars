using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.FSM
{
    /// <summary>
    /// 有限状态机状态基类
    /// </summary>
    public abstract class BaseFSMState
    {
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

