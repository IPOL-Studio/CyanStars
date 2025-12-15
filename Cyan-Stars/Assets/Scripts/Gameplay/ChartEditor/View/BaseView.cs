using System;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public abstract class BaseView : MonoBehaviour
    {
        private BaseViewModel viewModel;


        public virtual void Bind(BaseViewModel targetViewModel)
        {
            viewModel = targetViewModel;
        }


        protected virtual void Start()
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel), "此 view 尚未绑定 viewModel。如果是静态 view，请在 MvvmBindManager 中添加绑定；如果是动态 view，请在创建预制体实例后立刻调用其方法绑定 viewModel。");
            }
        }

        // 提醒子类强制实现 Destroy，主要用于取消订阅关系
        protected abstract void OnDestroy();
    }
}
