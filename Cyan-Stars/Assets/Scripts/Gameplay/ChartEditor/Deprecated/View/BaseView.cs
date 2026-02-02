// using CyanStars.GamePlay.ChartEditor.Model;
// using UnityEngine;
//
// namespace CyanStars.GamePlay.ChartEditor.View
// {
//     public abstract class BaseView : MonoBehaviour
//     {
//         protected ChartEditorModel Model;
//
//         /// <summary>
//         /// 为 View 绑定一个已经初始化的 Model 实例
//         /// </summary>
//         /// <remarks>重写时添加：从 Model 获取初始值并刷新 UI、监听 UI 变化事件并调用 Model 方法、监听 Model 事件并刷新 UI。别忘了在 OnDestroy() 中取消订阅事件，以免内存泄漏</remarks>
//         /// <param name="chartEditorModel">Model 实例</param>
//         public virtual void Bind(ChartEditorModel chartEditorModel) // TODO: 重构代码，让组件在 Awake() 时向上查找 editorModel，而非手动绑定
//         {
//             Model = chartEditorModel;
//         }
//     }
// }
