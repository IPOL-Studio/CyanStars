using System.Threading.Tasks;
using UnityEngine;

namespace CyanStars.Framework.UI
{
    public partial class UIManager
    {
        /// <summary>
        /// 打开UI（可等待）
        /// </summary>
        public Task<T> AwaitOpenUIPanel<T>() where T : BaseUIPanel
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            OpenUIPanel<T>(uiPanel => tcs.SetResult(uiPanel));
            return tcs.Task;
        }

        /// <summary>
        /// 使用预制体名获取UIItem（可等待）
        /// </summary>
        public Task<T> AwaitGetUIItem<T>(string prefabName, Transform parent) where T : BaseUIItem
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            GetUIItem<T>(prefabName, parent, item => tcs.SetResult(item));
            return tcs.Task;
        }

        /// <summary>
        /// 使用模板获取UIItem（可等待）
        /// </summary>
        public Task<T> AwaitGetUIItem<T>(GameObject template, Transform parent) where T : BaseUIItem
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            GetUIItem<T>(template, parent, item => tcs.SetResult(item));
            return tcs.Task;
        }
    }
}
