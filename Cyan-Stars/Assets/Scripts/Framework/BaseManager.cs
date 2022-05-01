using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework
{
    /// <summary>
    /// 游戏管理器基类
    /// </summary>
    public abstract class BaseManager : MonoBehaviour
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public abstract int  Priority { get; }
        
        /// <summary>
        /// 初始化管理器
        /// </summary>
        public abstract void OnInit();
        
        /// <summary>
        /// 轮询管理器
        /// </summary>
        public abstract void OnUpdate(float deltaTime);

        protected void Awake()
        {
            GameRoot.RegisterManager(this);
        }
    }

}

