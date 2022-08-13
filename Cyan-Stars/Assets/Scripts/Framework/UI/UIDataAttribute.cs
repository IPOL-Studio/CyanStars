using System;

namespace CyanStars.Framework.UI
{
    /// <summary>
    /// UI数据特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UIDataAttribute : Attribute
    {
        /// <summary>
        /// UI预制体名
        /// </summary>
        public string UIPrefabName;

        /// <summary>
        /// UI组名
        /// </summary>
        public string UIGroupName;

        /// <summary>
        /// 是否允许打开多个UI实例
        /// </summary>
        public bool AllowMultiple;
    }
}
