using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CyanStars.Framework.Asset;
using CyanStars.Framework.FSM;
using CyanStars.Framework.GameObjectPool;
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
        /// 资源管理器
        /// </summary>
        public static AssetManager Asset;
        
        /// <summary>
        /// 有效状态机管理器
        /// </summary>
        public static FSMManager FSM;

        /// <summary>
        /// 游戏对象池
        /// </summary>
        public static GameObjectPoolManager GameObjectPool;
        
        /// <summary>
        /// 流程状态机
        /// </summary>
        private static FSM.FSM procedureFSM;
        
        private async void Start()
        {
            //按优先级排序并初始化所有Manager
            managers.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].OnInit();
            }

            FSM = GetManager<FSMManager>();
            Asset = GetManager<AssetManager>();
            GameObjectPool = GetManager<GameObjectPoolManager>();
            
            //启动游戏流程
            GameProcedureStartUp();
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
            for (int i = 0; i < managers.Count; i++)
            {
                if (typeof(T) == managers[i].GetType())
                {
                    return (T)managers[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 启动游戏流程
        /// </summary>
        private static void GameProcedureStartUp()
        {
           Type[] types = typeof(GameRoot).Assembly.GetTypes();
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

           
           procedureFSM = FSM.CreateFSM(procedureStates, entryProcedureType);
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

