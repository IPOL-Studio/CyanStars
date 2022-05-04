using System.Threading.Tasks;

namespace CyanStars.Framework.UI
{
    /// <summary>
    /// 可等待扩展
    /// </summary>
    public static class AwaitableExtension
    {
        /// <summary>
        /// 打开UI（可等待）
        /// </summary>
        public static Task<T> AwaitOpenUI<T>(this UIManager self) where T : BaseUIPanel
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            
            self.OpenUI<T>((uiPanel) =>
            {
                tcs.SetResult(uiPanel);
            });

            return tcs.Task;
        }
    }
}