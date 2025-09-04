using CyanStars.Chart;

namespace CyanStars.ChartEditor.Model
{
    /// <summary>
    /// 用于 M 层的 Note 数据
    /// </summary>
    public class ModelChartNoteData
    {
        /// <summary>
        /// 音符 ID，需要保证此 ID 与 NoteView 中 ID 一致
        /// </summary>
        /// <remarks>ID 字段不会被持久化，每次编辑器加载谱面时遍历分配
        /// 考虑到性能问题，此字段存入列表时：
        /// 列表中每个 ID 需要保证唯一、顺序性（便于高效查找，需要在插入时确认合适位置，通常在列表尾部，撤销操作除外），
        /// 但不需要保证连续（允许删除中间元素而不重载 ID）。</remarks>

        public int ID { get; }

        public BaseChartNoteData NoteData { get; }

        public ModelChartNoteData(int id, BaseChartNoteData noteData)
        {
            ID = id;
            NoteData = noteData ?? throw new System.ArgumentNullException(nameof(noteData));
        }
    }
}
