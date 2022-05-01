using System;
using System.Collections.Generic;

namespace CyanStars.Framework.FSM
{
    /// <summary>
    /// 有限状态机
    /// </summary>
    public class FSM
    {
        /// <summary>
        /// 状态字典
        /// </summary>
        private Dictionary<Type, BaseFSMState> stateDict = new Dictionary<Type, BaseFSMState>();

        /// <summary>
        /// 当前状态
        /// </summary>
        private BaseFSMState currentState;
        
        public FSM(List<BaseFSMState> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                BaseFSMState state = states[i];
                stateDict.Add(state.GetType(),state);
            }
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState<T>() where T : BaseFSMState
        {
            Type type = typeof(T);
            
            if (!stateDict.TryGetValue(type,out BaseFSMState state))
            {
                throw new Exception($"状态切换失败，没有此状态：{type}");
            }
            
            currentState.OnExit();
            currentState = state;
            currentState.OnEnter();
        }
        
        /// <summary>
        /// 轮询有限状态机
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            currentState.OnUpdate(deltaTime);
        }
    }

}
