using CyanStars.Chart;
using CyanStars.ChartEditor.Model;

namespace CyanStars.ChartEditor.View
{
    /// <summary>
    /// 编辑器音符类
    /// </summary>
    public class EditorNote : BaseView
    {
        public NoteType NoteType;
        public BaseChartNoteData Data;


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);
        }

        /// <summary>
        /// 在创建/从对象池取回此物体时初始化数据
        /// </summary>
        /// <param name="noteType">音符类型</param>
        /// <param name="data">音符数据，用于哈希查询</param>
        public void Init(NoteType noteType, BaseChartNoteData data)
        {
            NoteType = noteType;
            Data = data;
        }
    }
}
