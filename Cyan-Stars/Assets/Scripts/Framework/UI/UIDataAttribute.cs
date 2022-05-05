using System;

namespace CyanStars.Framework.UI
{
    /// <summary>
    /// UI数据特性
    /// </summary>
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
        

    }
}