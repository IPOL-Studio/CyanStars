using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.UI
{
    /// <summary>
    /// UI组
    /// </summary>
    [Serializable]
    public class UIGroup
    {
        /// <summary>
        /// UI界面列表
        /// </summary>
        private List<BaseUIPanel> uiPanels = new List<BaseUIPanel>();

        /// <summary>
        /// UI根节点
        /// </summary>
        public Canvas UIRoot;

        /// <summary>
        /// UI组名
        /// </summary>
        public string Name;

        /// <summary>
        /// UI界面深度间隔
        /// </summary>
        public int UIPanelDepthStep = 1;

        /// <summary>
        /// 深度，值越大越在顶端
        /// </summary>
        public int Depth
        {
            get => UIRoot.sortingOrder;
            set => UIRoot.sortingOrder = value;
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        public void OpenUIPanel<T>(UIDataAttribute uiData, Action<T> callback) where T : BaseUIPanel
        {
            GameRoot.GameObjectPool.GetGameObject(uiData.UIPrefabName, UIRoot.transform, (go) =>
            {
                //go.transform.SetParent(UIRoot.transform);

                BaseUIPanel uiPanel = go.GetComponent<BaseUIPanel>();
                uiPanels.Add(uiPanel);

                uiPanel.Depth = Depth + (uiPanels.Count * UIPanelDepthStep); //重新计算深度

                uiPanel.OnOpen();

                callback?.Invoke((T)uiPanel);
            });
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public void CloseUI(UIDataAttribute uiData, BaseUIPanel uiPanel)
        {
            uiPanel.OnClose();

            uiPanel.Depth = 0;

            int index = uiPanels.IndexOf(uiPanel);
            bool isTopUI = index == uiPanels.Count - 1;
            uiPanels.RemoveAt(index);

            GameRoot.GameObjectPool.ReleaseGameObject(uiData.UIPrefabName, uiPanel.gameObject);

            //关闭并非最顶端UI后要重新计算深度
            if (!isTopUI)
            {
                ReCalUIDepth();
            }
        }

        /// <summary>
        /// 重新计算所有UI面板的深度
        /// </summary>
        private void ReCalUIDepth()
        {
            for (int i = 0; i < uiPanels.Count; i++)
            {
                BaseUIPanel uiPanel = uiPanels[i];
                uiPanel.Depth = Depth + (i * UIPanelDepthStep);
            }
        }

        /// <summary>
        /// 获取UI
        /// </summary>
        public T GetUIPanel<T>() where T : BaseUIPanel
        {
            for (int i = 0; i < uiPanels.Count; i++)
            {
                BaseUIPanel uiPanel = uiPanels[i];
                if (uiPanel.GetType() == typeof(T))
                {
                    return (T)uiPanel;
                }
            }

            return null;
        }
    }
}
