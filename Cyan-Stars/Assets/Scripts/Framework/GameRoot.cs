using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CyanStars.Framework.UI;
using CyanStars.Framework.FSM;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Audio;
using CyanStars.Framework.Event;
using CyanStars.Framework.File;
using CyanStars.Framework.GameObjectPool;
using CyanStars.Framework.Logging;
using CyanStars.Framework.Timer;


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
        private static readonly List<BaseManager> Managers = new List<BaseManager>();

        /// <summary>
        /// 数据模块字典
        /// </summary>
        private static readonly Dictionary<Type, BaseDataModule>
            DataModuleDict = new Dictionary<Type, BaseDataModule>();

        /// <summary>
        /// 主相机
        /// </summary>
        public static Camera MainCamera { get; private set; }

        /// <summary>
        /// 资源管理器
        /// </summary>
        public static AssetManager Asset { get; private set; }

        /// <summary>
        /// 音频管理器
        /// </summary>
        public static AudioManager Audio { get; private set; }

        /// <summary>
        /// 事件管理器
        /// </summary>
        public static EventManager Event { get; private set; }

        /// <summary>
        /// 有限状态机管理器
        /// </summary>
        public static FSMManager FSM { get; private set; }

        /// <summary>
        /// 游戏对象池管理器
        /// </summary>
        public static GameObjectPoolManager GameObjectPool { get; private set; }

        /// <summary>
        /// 定时器管理器
        /// </summary>
        public static TimerManager Timer { get; private set; }

        /// <summary>
        /// UI管理器
        /// </summary>
        public static UIManager UI { get; private set; }

        /// <summary>
        /// Logger管理器
        /// </summary>
        public static LoggerManager Logger { get; private set; }

        /// <summary>
        /// 用户数据文件管理器
        /// </summary>
        public static FileManager File { get; private set; }

        /// <summary>
        /// 流程状态机
        /// </summary>
        private static FSM.FSM procedureFSM;

        /// <summary>
        /// 需要注册到框架中的程序集列表
        /// </summary>
        [Header("需要注册到框架中的程序集列表")]
        public List<string> RegisterAssemblies;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            MainCamera = Camera.main;
            Asset = GetManager<AssetManager>();
            Audio = GetManager<AudioManager>();
            Event = GetManager<EventManager>();
            FSM = GetManager<FSMManager>();
            GameObjectPool = GetManager<GameObjectPoolManager>();
            Timer = GetManager<TimerManager>();
            UI = GetManager<UIManager>();
            Logger = GetManager<LoggerManager>();
            File = GetManager<FileManager>();

            //按优先级排序并初始化所有Manager
            Managers.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            for (int i = 0; i < Managers.Count; i++)
            {
                Managers[i].OnInit();
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                if (RegisterAssemblies.Contains(assembly.GetName().Name))
                {
                    types.AddRange(assembly.GetTypes());
                }
            }

            if (types.Count > 0)
            {
                //初始化数据模块
                InitDataModules(types);

                //启动游戏流程
                GameProcedureStartUp(types);
            }
        }

        private void Update()
        {
            //轮询所有Manager
            for (int i = 0; i < Managers.Count; i++)
            {
                Managers[i].OnUpdate(Time.deltaTime);
            }
        }

        /// <summary>
        /// 注册管理器
        /// </summary>
        public static void RegisterManager(BaseManager manager)
        {
            Managers.Add(manager);
        }

        /// <summary>
        /// 获取指定类型的游戏管理器
        /// </summary>
        public static T GetManager<T>() where T : BaseManager
        {
            Type type = typeof(T);
            for (int i = 0; i < Managers.Count; i++)
            {
                if (type == Managers[i].GetType())
                {
                    return (T)Managers[i];
                }
            }

            Debug.LogError($"要获取的管理器不存在{type.Name}");
            return null;
        }

        /// <summary>
        /// 初始化数据模块
        /// </summary>
        private static void InitDataModules(List<Type> types)
        {
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(BaseDataModule)))
                {
                    BaseDataModule dataModule = (BaseDataModule)Activator.CreateInstance(type);
                    DataModuleDict.Add(type, dataModule);
                    dataModule.OnInit();
                }
            }
        }

        /// <summary>
        /// 获取数据模块
        /// </summary>
        public static T GetDataModule<T>() where T : BaseDataModule
        {
            Type type = typeof(T);
            if (!DataModuleDict.TryGetValue(type, out BaseDataModule dataModule))
            {
                Debug.LogError($"要获取的数据模块不存在:{type.Name}");
                return null;
            }

            return (T)dataModule;
        }

        /// <summary>
        /// 启动游戏流程
        /// </summary>
        private static void GameProcedureStartUp(List<Type> types)
        {
            List<BaseState> procedureStates = new List<BaseState>();
            Type entryProcedureType = null;

            foreach (Type type in types)
            {
                //检查是否为流程状态
                if (type.IsSubclassOf(typeof(BaseState)) && Attribute.IsDefined(type, typeof(ProcedureStateAttribute)))
                {
                    ProcedureStateAttribute procedureAttr =
                        (ProcedureStateAttribute)Attribute.GetCustomAttribute(type, typeof(ProcedureStateAttribute));
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

            if (procedureStates.Count == 0)
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
