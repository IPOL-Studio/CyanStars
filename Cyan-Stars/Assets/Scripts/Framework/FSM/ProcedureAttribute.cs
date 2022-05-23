using System;

namespace CyanStars.Framework.FSM
{
    /// <summary>
    /// 流程状态特性
    /// </summary>
    public class ProcedureStateAttribute : Attribute
    {
        /// <summary>
        /// 是否为入口流程
        /// </summary>
        public readonly bool IsEntryProcedure;

        public ProcedureStateAttribute(bool isEntryProcedure = false)
        {
            IsEntryProcedure = isEntryProcedure;
        }
    }
}
