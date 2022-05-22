using System;
using System.Collections.Generic;
using CyanStars.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Framework.UI
{
    /// <summary>
    /// UI面板基类
    /// </summary>
    public abstract class BaseUIPanel : MonoBehaviour
    {
        /// <summary>
        /// UI面板自身的Canvas，主要用来控制深度
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// 显示中的UIItem列表
        /// </summary>
        private List<BaseUIItem> showingUIItems = new List<BaseUIItem>();

        /// <summary>
        /// 深度，值越大越在顶端
        /// </summary>
        public int Depth
        {
            get => canvas.sortingOrder;
            set => canvas.sortingOrder = value;
        }
        
        private void Awake()
        {
            canvas = gameObject.GetOrAddComponent<Canvas>();
            canvas.overrideSorting = true;

            gameObject.AddComponent<GraphicRaycaster>();
            
            OnCreate();
        }

        /// <summary>
        /// 创建UI面板时调用（通常于此时进行一些初始化操作）
        /// </summary>
        protected virtual void OnCreate()
        {
            
        }

        /// <summary>
        /// 打开UI面板时调用（通常于此时刷新数据）
        /// </summary>
        public virtual void OnOpen()
        {
        }
        
        /// <summary>
        /// 关闭UI面板时调用
        /// </summary>
        public virtual void OnClose()
        {
            //UI面板被关闭时 归还所有Item到对象池中
            foreach (BaseUIItem item in showingUIItems)
            {
                ReleaseUIItem(item);
            }
            showingUIItems.Clear();
        }

        /// <summary>
        /// 销毁UI面板时调用
        /// </summary>
        public virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 使用预制体名获取UIItem
        /// </summary>
        public void GetUIItem<T>(string prefabName,Transform parent,Action<T> callback) where T : BaseUIItem
        {
            GameRoot.GameObjectPool.GetGameObject(prefabName,parent, (go) =>
            {
                T item = OnGetUIItem(callback, go);

                item.PrefabName = prefabName;
            });
        }
        
        /// <summary>
        /// 使用模板获取UIItem
        /// </summary>
        public void GetUIItem<T>(GameObject itemTemplate,Transform parent,Action<T> callback) where T : BaseUIItem
        {
            GameRoot.GameObjectPool.GetGameObject(itemTemplate,parent, (go) =>
            {
                T item = OnGetUIItem(callback, go);
                
                item.Template = itemTemplate;
            });
        }

        /// <summary>
        /// 从对象池中获取UIItem时调用
        /// </summary>
        private T OnGetUIItem<T>(Action<T> callback, GameObject go) where T : BaseUIItem
        {
            T item = go.GetComponent<T>();
            item.OnShow();
            showingUIItems.Add(item);
            callback?.Invoke(item);
            return item;
        }

        /// <summary>
        /// 将UIItem归还到对象池中
        /// </summary>
        protected void ReleaseUIItem(BaseUIItem item)
        {
            item.OnHide();
            
            if (string.IsNullOrEmpty(item.PrefabName))
            {
                GameRoot.GameObjectPool.ReleaseGameObject(item.PrefabName,item.gameObject);   
            }
            else
            {
                GameRoot.GameObjectPool.ReleaseGameObject(item.Template,item.gameObject);   
            }

            item.PrefabName = null;
            item.Template = null;

        }
        

    }
}