using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    public static class DialogueEventConst
    {
        /// <summary>
        /// 切换对话事件
        /// </summary>
        public const string GalSwitchDialog = "GalSwitchDialog";

        /// <summary>
        /// 执行缓动效果事件
        /// </summary>
        public const string GalAnimation = "GalAnimation";

        /// <summary>
        /// 跳过缓动效果事件
        /// </summary>
        public const string GalSkipAnimation = "GalSkipAnimation";

        /// <summary>
        /// 创建分枝UI事件
        /// </summary>
        public const string GalCreateBranchUI = "GalCreateBranchUI";
    }
}
