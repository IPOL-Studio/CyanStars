using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.FSM
{
    /// <summary>
    /// 有限状态机管理器
    /// </summary>
    public class FSMManager : BaseManager
    {
        /// <inheritdoc />
        public override int Priority { get; }

        /// <summary>
        /// 所有有限状态机
        /// </summary>
        private List<FSM> fsms = new List<FSM>();
        
        /// <summary>
        /// 等待销毁的状态机列表
        /// </summary>
        private HashSet<FSM> waitRemoveFSMs = new HashSet<FSM>();

        /// <inheritdoc />
        public override void OnInit()
        {

        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {

            //轮询所有状态机
            for (int i = 0; i < fsms.Count; i++)
            {
                FSM fsm = fsms[i];
                if (fsm.IsDestroyed)
                {
                    continue;
                }
                
                fsm.OnUpdate(deltaTime);
            }

            //移除等待移除的状态机
            if (waitRemoveFSMs.Count == 0)
            {
                return;
            }
            foreach (FSM fsm in waitRemoveFSMs)
            {
                fsms.Remove(fsm);
            }
            waitRemoveFSMs.Clear();
        }

        
        /// <summary>
        /// 创建有限状态机
        /// </summary>
        public FSM CreateFSM(List<BaseState> states)
        {
            FSM fsm = new FSM(states);
            fsms.Add(fsm);
            return fsm;
        }

        /// <summary>
        /// 销毁有限状态机
        /// </summary>
        public void DestroyFSM(FSM fsm)
        { 
            fsm.OnDestroy(); 
            
            //因为是正向遍历所有状态机调用OnUpdate的，所有要删除的话不能在这里删除
            //否则在fsm.OnUpdate中销毁自身就会出问题
            waitRemoveFSMs.Add(fsm);
        }
        
    }
}

