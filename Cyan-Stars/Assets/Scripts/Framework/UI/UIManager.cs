using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.UI
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager : BaseManager
    {
        [Header("UI相机")]
        public Camera UICamera;

        /// <summary>
        /// UI组深度间隔
        /// </summary>
        [Header(" UI组深度间隔")]
        public int UIGroupDepthStep = 100;

        /// <summary>
        /// UI组列表
        /// </summary>
        [Header("UI组列表")]
        public List<UIGroup> UIGroups;

        /// <summary>
        /// UI组名->UI组
        /// </summary>
        private readonly Dictionary<string, UIGroup> uiGroupDict = new Dictionary<string, UIGroup>();

        /// <summary>
        /// UI面板->UI面板数据
        /// </summary>
        private readonly Dictionary<Type, UIDataAttribute> uiDataDict = new Dictionary<Type, UIDataAttribute>();

        /// <inheritdoc />
        public override int Priority { get; }

        /// <inheritdoc />
        public override void OnInit()
        {
            //计算各UI组深度
            for (int i = 0; i < UIGroups.Count; i++)
            {
                UIGroup group = UIGroups[i];
                uiGroupDict.Add(group.Name, group);

                group.Depth = i * UIGroupDepthStep;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        public void OpenUIPanel<T>(Action<T> callback) where T : BaseUIPanel
        {
            UIGroup uiGroup = GetUIGroup<T>(out UIDataAttribute uiData);
            uiGroup.OpenUIPanel(uiData, callback);
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public void CloseUIPanel<T>() where T : BaseUIPanel
        {
            UIGroup uiGroup = GetUIGroup<T>(out UIDataAttribute uiData);
            T uiPanel = uiGroup.GetUIPanel<T>();
            CloseUIPanel(uiPanel);
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public void CloseUIPanel<T>(T uiPanel) where T : BaseUIPanel
        {
            UIGroup uiGroup = GetUIGroup<T>(out UIDataAttribute uiData);
            uiGroup.CloseUIPanel(uiData, uiPanel);
        }

        /// <summary>
        /// 获取UI面板
        /// </summary>
        public T GetUIPanel<T>() where T : BaseUIPanel
        {
            UIGroup uiGroup = GetUIGroup<T>(out UIDataAttribute uiData);
            return uiGroup.GetUIPanel<T>();
        }

        /// <summary>
        /// 获取UI组
        /// </summary>
        private UIGroup GetUIGroup<T>(out UIDataAttribute uiData) where T : BaseUIPanel
        {
            uiData = GetOrAddUIPanelData<T>();

            if (!uiGroupDict.TryGetValue(uiData.UIGroupName, out UIGroup group))
            {
                throw new Exception($"UI面板{typeof(T).Name}的UI组{uiData.UIGroupName}未定义");
            }

            return group;
        }

        /// <summary>
        /// 获取UI面板数据，若不存在则添加
        /// </summary>
        private UIDataAttribute GetOrAddUIPanelData<T>() where T : BaseUIPanel
        {
            Type type = typeof(T);
            if (!uiDataDict.TryGetValue(type, out UIDataAttribute uiData))
            {
                Type attr = typeof(UIDataAttribute);
                if (!Attribute.IsDefined(type, attr))
                {
                    throw new Exception($"要获取UI面板数据的UI面板未标记UIPanelData特性:{type.Name}");
                }

                uiData = (UIDataAttribute)Attribute.GetCustomAttribute(type, attr);
                uiDataDict.Add(type, uiData);
            }

            return uiData;
        }

        /// <summary>
        /// 使用预制体名获取UIItem
        /// </summary>
        public void GetUIItem<T>(string prefabName, Transform parent, Action<T> callback) where T : BaseUIItem
        {
            GameRoot.GameObjectPool.GetGameObject(prefabName, parent, (go) =>
            {
                T item = OnGetUIItem(callback, go);

                item.PrefabName = prefabName;
            });
        }

        /// <summary>
        /// 使用模板获取UIItem
        /// </summary>
        public void GetUIItem<T>(GameObject itemTemplate, Transform parent, Action<T> callback) where T : BaseUIItem
        {
            GameRoot.GameObjectPool.GetGameObject(itemTemplate, parent, (go) =>
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
            item.OnGet();
            callback?.Invoke(item);
            return item;
        }

        /// <summary>
        /// 将列表中的UIItem归还到对象池中
        /// </summary>
        public void ReleaseUIItems(List<BaseUIItem> items)
        {
            //归还所有Item到对象池中
            foreach (BaseUIItem item in items)
            {
                ReleaseUIItem(item);
            }

            items.Clear();
        }


        /// <summary>
        /// 将UIItem归还到对象池中
        /// </summary>
        public void ReleaseUIItem(BaseUIItem item)
        {
            item.OnRelease();

            if (!string.IsNullOrEmpty(item.PrefabName))
            {
                GameRoot.GameObjectPool.ReleaseGameObject(item.PrefabName, item.gameObject);
            }
            else
            {
                GameRoot.GameObjectPool.ReleaseGameObject(item.Template, item.gameObject);
            }

            item.PrefabName = null;
            item.Template = null;
        }
    }
}
