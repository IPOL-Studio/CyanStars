using System;

namespace CyanStars.Framework.Dialogue
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DialogueStepAttribute : Attribute
    {
        /// <summary>
        /// 在Json文件中使用的Type字段
        /// </summary>
        public string StepType { get; }

        /// <summary>
        /// 是否允许同时执行多个Step
        /// </summary>
        public bool AllowMultiple { get; set; }

        public DialogueStepAttribute(string stepType)
        {
            this.StepType = stepType;
        }

    }
}
