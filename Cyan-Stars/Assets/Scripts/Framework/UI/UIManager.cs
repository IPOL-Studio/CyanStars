using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly Dictionary<string, UIGroup> UIGroupDict = new Dictionary<string, UIGroup>();

        /// <summary>
        /// UI面板->UI面板数据
        /// </summary>
        private readonly Dictionary<Type, UIDataAttribute> UIDataDict = new Dictionary<Type, UIDataAttribute>();

        /// <summary>
        /// 处于打开状态的UI面板 -> 打开的面板数量
        /// </summary>
        private readonly Dictionary<Type, int> OpenedUIDict = new Dictionary<Type, int>();

        /// <inheritdoc />
        public override int Priority { get; }

        /// <inheritdoc />
        public override void OnInit()
        {
            //计算各UI组深度
            for (int i = 0; i < UIGroups.Count; i++)
            {
                UIGroup group = UIGroups[i];
                UIGroupDict.Add(group.Name, group);

                group.Depth = i * UIGroupDepthStep;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        public ValueTask<T> OpenUIPanelAsync<T>() where T : BaseUIPanel
        {
            Type type = typeof(T);
            if (!InternalOpenUIPanel(type,out var uiGroup,out var uiData))
            {
                return default;
            }
            return uiGroup.OpenUIPanelAsync<T>(uiData);
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        public ValueTask<BaseUIPanel> OpenUIPanelAsync(Type type)
        {
            if (!InternalOpenUIPanel(type,out var uiGroup,out var uiData))
            {
                return default;
            }
            return uiGroup.OpenUIPanelAsync(uiData);
        }

        private bool InternalOpenUIPanel(Type type, out UIGroup uiGroup,out UIDataAttribute uiData)
        {
            uiGroup = GetUIGroup(type, out uiData);
            bool uiPanelOpened = OpenedUIDict.TryGetValue(type, out int count);
            if (!uiData.AllowMultiple && uiPanelOpened)
            {
                Debug.LogWarning($"只允许打开UI面板{type.Name}的一个实例");
                return false;
            }

            OpenedUIDict[type] = count + 1;
            return true;
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public void CloseUIPanel<T>() where T : BaseUIPanel
        {
           CloseUIPanel(typeof(T));
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public void CloseUIPanel(Type type)
        {
            UIGroup uiGroup = GetUIGroup(type, out _);
            BaseUIPanel uiPanel = uiGroup.GetUIPanel(type);
            CloseUIPanel(uiPanel);
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public void CloseUIPanel(BaseUIPanel uiPanel)
        {
            Type type = uiPanel.GetType();
            UIGroup uiGroup = GetUIGroup(type,out UIDataAttribute uiData);
            uiGroup.CloseUIPanel(uiData, uiPanel);

            if (!OpenedUIDict.TryGetValue(type, out int count)) return;
            count--;
            if (count == 0)
            {
                OpenedUIDict.Remove(type);
            }
            else
            {
                OpenedUIDict[type] = count;
            }
        }

        /// <summary>
        /// 获取UI面板
        /// </summary>
        public T GetUIPanel<T>() where T : BaseUIPanel
        {
            Type type = typeof(T);
            return (T)GetUIPanel(type);
        }

        /// <summary>
        /// 获取UI面板
        /// </summary>
        public BaseUIPanel GetUIPanel(Type type)
        {
            UIGroup uiGroup = GetUIGroup(type, out _);
            return uiGroup.GetUIPanel(type);
        }

        /// <summary>
        /// 获取UI组
        /// </summary>
        private UIGroup GetUIGroup(Type type, out UIDataAttribute uiData)
        {
            uiData = GetOrAddUIPanelData(type);
            if (!UIGroupDict.TryGetValue(uiData.UIGroupName, out UIGroup group))
            {
                throw new Exception($"UI面板{type.Name}的UI组{uiData.UIGroupName}未定义");
            }

            return group;
        }


        /// <summary>
        /// 获取UI面板数据，若不存在则添加
        /// </summary>
        private UIDataAttribute GetOrAddUIPanelData(Type type)
        {
            if (!UIDataDict.TryGetValue(type, out UIDataAttribute uiData))
            {
                Type attr = typeof(UIDataAttribute);
                if (!Attribute.IsDefined(type, attr))
                {
                    throw new Exception($"要获取UI面板数据的UI面板未标记UIPanelData特性:{type.Name}");
                }

                uiData = (UIDataAttribute)Attribute.GetCustomAttribute(type, attr);
                UIDataDict.Add(type, uiData);
            }

            return uiData;
        }

        /// <summary>
        /// 使用预制体名获取UIItem
        /// </summary>
        public async ValueTask<T> GetUIItemAsync<T>(string prefabName, Transform parent) where T : BaseUIItem
        {
            var go = await GameRoot.GameObjectPool.GetGameObjectAsync(prefabName, parent);
            T item = OnGetUIItem<T>(go);
            item.PrefabName = prefabName;
            return item;
        }


        /// <summary>
        /// 使用模板获取UIItem
        /// </summary>
        public async ValueTask<T> GetUIItemAsync<T>(GameObject itemTemplate, Transform parent) where T : BaseUIItem
        {
            var go = await GameRoot.GameObjectPool.GetGameObjectAsync(itemTemplate, parent);
            T item = OnGetUIItem<T>(go);
            item.Template = itemTemplate;
            return item;
        }

        /// <summary>
        /// 从对象池中获取UIItem时调用
        /// </summary>
        private T OnGetUIItem<T>(GameObject go) where T : BaseUIItem
        {
            T item = go.GetComponent<T>();
            item.OnGet();
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
