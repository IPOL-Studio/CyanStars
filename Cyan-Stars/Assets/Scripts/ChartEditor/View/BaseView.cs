using CyanStars.ChartEditor.ViewModel;
using UnityEngine;

namespace CyanStars.ChartEditor.View
{
    /// <summary>
    /// 由管理器绑定 VM 和物体实例并初始化
    /// </summary>
    /// <param name="viewModel">VM 实例</param>
    public abstract class BaseView : MonoBehaviour
    {
        protected MainViewModel mainViewModel;

        public virtual void Bind(MainViewModel viewModel)
        {
            mainViewModel = viewModel;
        }
    }
}
