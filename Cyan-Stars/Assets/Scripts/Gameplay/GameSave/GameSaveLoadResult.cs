namespace CyanStars.Gameplay.GameSave
{
    /// <summary>
    /// 存档读取结果
    /// </summary>
    public enum GameSaveLoadResult
    {
        Success,
        FileNotFound,
        InvalidData,
        UnknownError
    }
}
