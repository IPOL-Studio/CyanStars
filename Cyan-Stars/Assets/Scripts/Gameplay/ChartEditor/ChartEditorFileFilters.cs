#nullable enable

using CyanStars.Framework.File;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 制谱器使用的文件类型过滤器
    /// </summary>
    public static class ChartEditorFileFilters
    {
        /// <summary>
        /// 图片（.png）
        /// </summary>
        public static readonly FileTypeFilter Sprite = new FileTypeFilter("图片", ".png");

        /// <summary>
        /// 音频（.ogg）
        /// </summary>
        public static readonly FileTypeFilter Audio = new FileTypeFilter("音频", ".ogg");
    }
}
