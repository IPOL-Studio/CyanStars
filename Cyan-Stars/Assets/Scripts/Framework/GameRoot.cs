using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Event;
using CyanStars.Framework.FSM;
using CyanStars.Framework.GameObjectPool;
using CyanStars.Framework.UI;
using CyanStars.Gameplay;
using UnityEngine;

namespace CyanStars.Framework
{
    /// <summary>
    /// 游戏根节点
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        /// <summary>
        /// 游戏管理器列表
        /// </summary>
        private static List<BaseManager> managers = new List<BaseManager>();

        /// <summary>
        /// 数据模块字典
        /// </summary>
        private static Dictionary<Type,BaseDataModule> dataModuleDict = new Dictionary<Type,BaseDataModule>();

        /// <summary>
        /// 资源管理器
        /// </summary>
        public static AssetManager Asset;

        /// <summary>
        /// 事件管理器
        /// </summary>
        public static EventManager Event;
        
        /// <summary>
        /// 有限状态机管理器
        /// </summary>
        public static FSMManager FSM;

        /// <summary>
        /// 游戏对象池管理器
        /// </summary>
        public static GameObjectPoolManager GameObjectPool;

        /// <summary>
        /// UI管理器
        /// </summary>
        public static UIManager UI;
        
        /// <summary>
        /// 流程状态机
        /// </summary>
        private static FSM.FSM procedureFSM;

        [Header("是否开启自动模式")]
        public bool IsAutoMode;

        [Header("谱面文件名")]
        public string MusicGameDataName;
        
        private void Start()
        {
            Asset = GetManager<AssetManager>();
            Event = GetManager<EventManager>();
            FSM = GetManager<FSMManager>();
            GameObjectPool = GetManager<GameObjectPoolManager>();
            UI = GetManager<UIManager>();
            
            //按优先级排序并初始化所有Manager
            managers.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].OnInit();
            }
            
            Type[] types = typeof(GameRoot).Assembly.GetTypes();
            
            //初始化数据模块
            InitDataModules(types);
            
            //先暂时这样直接给参数
            GetDataModule<MusicGameModule>().IsAutoMode = IsAutoMode;
            GetDataModule<MusicGameModule>().MusicGameDataName = MusicGameDataName;
            
            //启动游戏流程
            GameProcedureStartUp(types);
        }

        private void Update()
        {
            //轮询所有Manager
            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].OnUpdate(Time.deltaTime);
            }
        }

        /// <summary>
        /// 注册管理器
        /// </summary>
        public static void RegisterManager(BaseManager manager)
        {
            managers.Add(manager);
        }

        /// <summary>
        /// 获取指定类型的游戏管理器
        /// </summary>
        public static T GetManager<T>() where T : BaseManager
        {
            Type type = typeof(T);
            for (int i = 0; i < managers.Count; i++)
            {
                if (type == managers[i].GetType())
                {
                    return (T)managers[i];
                }
            }

            Debug.LogError($"要获取的管理器不存在{type.Name}");
            return null;
        }

        /// <summary>
        /// 初始化数据模块
        /// </summary>
        private static void InitDataModules(Type[] types)
        {
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(BaseDataModule)))
                {
                    BaseDataModule dataModule = (BaseDataModule)Activator.CreateInstance(type);
                    dataModuleDict.Add(type,dataModule);
                    dataModule.OnInit();
                }
            }
        }

        /// <summary>
        /// 获取数据模块
        /// </summary>
        public static T GetDataModule<T>() where T: BaseDataModule
        {
            Type type = typeof(T);
            if (!dataModuleDict.TryGetValue(type,out BaseDataModule dataModule))
            {
                Debug.LogError($"要获取的数据模块不存在:{type.Name}");
                return null;
            }

            return (T)dataModule;
        }
        
        /// <summary>
        /// 启动游戏流程
        /// </summary>
        private static void GameProcedureStartUp(Type[] types)
        {

           List<BaseState> procedureStates = new List<BaseState>();
           Type entryProcedureType = null;
           
           foreach (Type type in types)
           {
               //检查是否为流程状态
               if (type.IsSubclassOf(typeof(BaseState)) && Attribute.IsDefined(type,typeof(ProcedureStateAttribute)))
               {
                  ProcedureStateAttribute procedureAttr = (ProcedureStateAttribute)Attribute.GetCustomAttribute(type, typeof(ProcedureStateAttribute));
                  BaseState procedureState = (BaseState)Activator.CreateInstance(type);
                  procedureStates.Add(procedureState);

                  //检查是否为入口流程
                  if (procedureAttr.IsEntryProcedure)
                  {
                      if (entryProcedureType != null)
                      {
                          throw new Exception($"指定了多个入口流程:{entryProcedureType},{type}");
                      }
                      entryProcedureType = type;
                  }
               }
           }

           if (procedureStates == null)
           {
               throw new Exception("未指定入口流程，游戏启动失败");
           }

           
           procedureFSM = FSM.CreateFSM(procedureStates);
           procedureFSM.ChangeState(entryProcedureType);
        }

        /// <summary>
        /// 切换流程
        /// </summary>
        public static void ChangeProcedure<T>() where T : BaseState
        {
            procedureFSM.ChangeState<T>();
        }
    }
}

