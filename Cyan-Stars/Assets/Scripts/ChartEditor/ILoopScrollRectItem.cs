namespace CyanStars.ChartEditor
{
    public interface ILoopScrollRectItem
    {
        /// <summary>
        /// 自动接收 LoopScrollRect 回调
        /// </summary>
        /// <remarks>所有 LoopScrollRect（无限滚动窗口）的元素都必须实现此回调，以避免报错</remarks>
        /// <remarks>如果不需要处理回调，大括号留空即可</remarks>
        /// <param name="index">自身元素的序号下标，会在每次进入可视范围（生成/从对象池取回）时调用</param>
        void ScrollCellIndex(int index);
    }
}
