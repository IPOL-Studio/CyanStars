using System;
using UnityEngine;

namespace CyanStars.Framework.UI
{
    /// <summary>
    /// UIItem基类
    /// </summary>
    public abstract class BaseUIItem : MonoBehaviour
    {
        [NonSerialized]
        public string PrefabName;

        [NonSerialized]
        public GameObject Template;

        private void Awake()
        {
            OnCreate();
        }

        /// <summary>
        /// 创建UIItem时调用
        /// </summary>
        public virtual void OnCreate()
        {
        }

        /// <summary>
        /// 获取UIItem时调用
        /// </summary>
        public virtual void OnGet()
        {
        }

        /// <summary>
        /// 释放UIItem时调用
        /// </summary>
        public virtual void OnRelease()
        {
        }

        /// <summary>
        /// 销毁UIItem时调用
        /// </summary>
        public virtual void OnDestroy()
        {
        }

    }
}
