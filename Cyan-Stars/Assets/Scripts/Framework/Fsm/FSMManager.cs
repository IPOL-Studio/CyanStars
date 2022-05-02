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

        private List<FSM> tempFSMs = new List<FSM>();

        /// <inheritdoc />
        public override void OnInit()
        {

        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {
            tempFSMs.Clear();
            for (int i = 0; i < fsms.Count; i++)
            {
                tempFSMs.Add(fsms[i]);
            }

            for (int i = 0; i < tempFSMs.Count; i++)
            {
                tempFSMs[i].OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 创建有限状态机
        /// </summary>
        public FSM CreateFSM<T>(List<BaseState> states) where T : BaseState
        {
            return CreateFSM(states,typeof(T));
        }

        /// <summary>
        /// 创建有限状态机
        /// </summary>
        public FSM CreateFSM(List<BaseState> states, Type entryStateType)
        {
            FSM fsm = new FSM(states);
            fsms.Add(fsm);
            fsm.ChangeState(entryStateType);
            return fsm;
        }

        /// <summary>
        /// 销毁有限状态机
        /// </summary>
        public bool DestroyFSM(FSM fsm)
        {
            return fsms.Remove(fsm);
        }
    }
}

