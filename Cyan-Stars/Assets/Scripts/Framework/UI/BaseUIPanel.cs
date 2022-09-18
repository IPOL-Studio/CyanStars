using CatAsset.Runtime;
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

        }

        /// <summary>
        /// 销毁UI面板时调用
        /// </summary>
        public virtual void OnDestroy()
        {
        }


    }
}
