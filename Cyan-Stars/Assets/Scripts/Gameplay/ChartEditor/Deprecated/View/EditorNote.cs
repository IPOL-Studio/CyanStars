// using CyanStars.Chart;
// using CyanStars.GamePlay.ChartEditor.Model;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace CyanStars.GamePlay.ChartEditor.View
// {
//     /// <summary>
//     /// 制谱器音符类
//     /// </summary>
//     public class EditorNote : BaseView
//     {
//         public RectTransform Rect;
//         public NoteType NoteType;
//         public RectTransform HoldTailRect;
//         public Button Button;
//
//         public BaseChartNoteData Data;
//
//
//         public override void Bind(ChartEditorModel chartEditorModel)
//         {
//             base.Bind(chartEditorModel);
//             Button.onClick.RemoveAllListeners();
//             Button.onClick.AddListener(Clicked);
//         }
//
//         /// <summary>
//         /// 在创建/从对象池取回此物体时初始化数据
//         /// </summary>
//         /// <param name="chartEditorModel">Model 实例</param>
//         /// <param name="data">音符数据，用于哈希查询</param>
//         public void SetData(ChartEditorModel chartEditorModel, BaseChartNoteData data)
//         {
//             Data = data;
//             Bind(chartEditorModel);
//         }
//
//         private void Clicked()
//         {
//             Model.SelectNote(this.Data);
//         }
//     }
// }



