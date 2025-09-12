using CyanStars.Chart;
using CyanStars.ChartEditor.Model;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    /// <summary>
    /// 编辑器音符类
    /// </summary>
    public class EditorNote : BaseView
    {
        public RectTransform Rect;
        public NoteType NoteType;
        public RectTransform HoldTailRect;
        public Button Button;

        public BaseChartNoteData Data;


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(Clicked);
        }

        /// <summary>
        /// 在创建/从对象池取回此物体时初始化数据
        /// </summary>
        /// <param name="editorModel">Model 实例</param>
        /// <param name="data">音符数据，用于哈希查询</param>
        public void SetData(EditorModel editorModel, BaseChartNoteData data)
        {
            Data = data;
            Bind(editorModel);
        }

        private void Clicked()
        {
            Model.SelectNote(this.Data);
        }
    }
}
