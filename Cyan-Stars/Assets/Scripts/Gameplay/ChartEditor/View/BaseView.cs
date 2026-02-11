#nullable enable

using System.Diagnostics.CodeAnalysis;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public abstract class BaseView<TViewModel> : MonoBehaviour where TViewModel : BaseViewModel
    {
        protected TViewModel ViewModel = null!;

        [MemberNotNull(nameof(ViewModel))] // 确保子类在 Bind 前被视为可 null 类型，尤其是动态生成的 View。Bind 方法调用后被视为不可 null 类型。
        public virtual void Bind(TViewModel targetViewModel)
        {
            ViewModel = targetViewModel;
        }

        // 提醒子类强制实现 Destroy，主要用于取消订阅关系
        protected abstract void OnDestroy();
    }
}
