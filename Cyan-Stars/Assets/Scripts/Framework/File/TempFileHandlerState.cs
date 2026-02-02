#nullable enable

namespace CyanStars.Framework.File
{
    public enum TempFileHandlerState
    {
        Saved = 0, // 文件副本已保存到目标路径
        Temped = 1, // 文件副本已保存到缓存路径，但未保存/未能成功保存到目标路径
        Unavailable = 2 // 句柄已释放/实例化出错，缓存文件可能不存在，相关属性可能不再有效
    }
}
