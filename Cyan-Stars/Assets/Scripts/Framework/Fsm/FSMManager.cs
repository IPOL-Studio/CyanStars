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

        /// <inheritdoc />
        public override void OnInit()
        {

        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {
            for (int i = fsms.Count - 1; i >= 0; i--)
            {
                fsms[i].OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 创建有限状态机
        /// </summary>
        public void CreateFSM<T>(List<BaseFSMState> states) where T : BaseFSMState
        {
            FSM fsm = new FSM(states);
            fsm.ChangeState<T>();
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

