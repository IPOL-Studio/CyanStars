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
        /// 显示UIItem时调用
        /// </summary>
        public virtual void OnShow()
        {
            
        }

        /// <summary>
        /// 隐藏UIItem时调用
        /// </summary>
        public virtual void OnHide()
        {
            
        }

        /// <summary>
        /// 销毁UIItem时调用
        /// </summary>
        public virtual void OnDestroy()
        {
            
        }

        
        /// <summary>
        /// 刷新Item
        /// </summary>
        public virtual void RefreshItem<T>(T data) where T : UIItemData
        {
            
        }
    }
}

