using System;

namespace CyanStars.Framework.Dialogue
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DialogueActionUnitAttribute : Attribute
    {
        /// <summary>
        /// 在Json文件中使用的Type字段
        /// </summary>
        public string ActionType { get; }

        /// <summary>
        /// 是否允许同时执行多个ActionUnit
        /// </summary>
        public bool AllowMultiple { get; set; }

        public DialogueActionUnitAttribute(string actionType)
        {
            this.ActionType = actionType;
        }

    }
}
