using System;
using System.Collections;
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
        public Camera UICamare;
        
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
        private Dictionary<string, UIGroup> uiGroupDict = new Dictionary<string, UIGroup>();

        /// <summary>
        /// UI面板->UI面板数据
        /// </summary>
        private Dictionary<Type, UIDataAttribute> uiDataDict =
            new Dictionary<Type, UIDataAttribute>();

        /// <inheritdoc />
        public override int Priority { get; }
        
        /// <inheritdoc />
        public override void OnInit()
        {
            for (int i = 0; i < UIGroups.Count; i++)
            {
                UIGroup group = UIGroups[i];
                uiGroupDict.Add(group.Name,group);

                group.Depth = i * UIGroupDepthStep;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        public void OpenUI<T>(Action<T> callback) where T : BaseUIPanel
        {
            UIGroup uiGroup = GetUIGroup<T>(out UIDataAttribute uiData);
            uiGroup.OpenUI(uiData,callback);
        }
        
        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public void CloseUI<T>() where T : BaseUIPanel
        {
            UIGroup uiGroup = GetUIGroup<T>(out UIDataAttribute uiData);
            T uiPanel = uiGroup.GetUI<T>();
            CloseUI(uiPanel);
        }
        
        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public void CloseUI<T>(T uiPanel) where T : BaseUIPanel
        {
            UIGroup uiGroup = GetUIGroup<T>(out UIDataAttribute uiData);
            uiGroup.CloseUI(uiData,uiPanel);
        }

        /// <summary>
        /// 获取UI组
        /// </summary>
        private UIGroup GetUIGroup<T>(out UIDataAttribute uiData) where T : BaseUIPanel
        {
            uiData = GetOrAddUIPanelData<T>();

            if (!uiGroupDict.TryGetValue(uiData.UIGroupName,out UIGroup group))
            {
                throw new Exception($"UI面板{typeof(T).Name}的UI组{uiData.UIGroupName}未定义");
            }

            return group;
        }
        
        /// <summary>
        /// 获取UI面板数据，若不存在则添加
        /// </summary>
        private UIDataAttribute GetOrAddUIPanelData<T>() where T:BaseUIPanel
        {
            Type type = typeof(T);
            if (!uiDataDict.TryGetValue(type,out UIDataAttribute uiData))
            {
                Type attr = typeof(UIDataAttribute);
                if (!Attribute.IsDefined(type,attr))
                {
                    throw new Exception($"要获取UI面板数据的UI面板未标记UIPanelData特性:{type.Name}");
                }

                uiData = (UIDataAttribute)Attribute.GetCustomAttribute(type, attr);
                uiDataDict.Add(type,uiData);
            }

            return uiData;
        }
    }

}
