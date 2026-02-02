#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 可缓存的文件的句柄（只读接口）
    /// </summary>
    public interface IReadonlyTempFileHandler
    {
        public TempFileHandlerState State { get; }
        public string OriginFilePath { get; }
        public string TempFilePath { get; }
        public string? TargetFilePath { get; }
    }
}
