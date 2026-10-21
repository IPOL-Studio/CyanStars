#nullable enable

using System;
using CyanStars.Utils;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件夹导入导出的路径策略
    /// </summary>
    internal static class FolderTransferPolicy
    {
        /// <summary>
        /// 校验源路径与目标路径是否符合传输方向
        /// </summary>
        /// <param name="sourceFolderPath">源文件夹绝对路径</param>
        /// <param name="targetFolderPath">目标文件夹绝对路径</param>
        /// <param name="direction">导入或导出</param>
        /// <param name="isManagedArea">判断路径是否位于应用自有目录内</param>
        /// <param name="error">失败原因；校验通过时为空字符串</param>
        /// <returns>路径组合是否符合方向要求</returns>
        public static bool TryValidate(
            string sourceFolderPath,
            string targetFolderPath,
            FolderTransferDirection direction,
            Func<string, bool> isManagedArea,
            out string error)
        {
            error = "";

            if (string.IsNullOrEmpty(sourceFolderPath) || string.IsNullOrEmpty(targetFolderPath))
            {
                error = "源路径或目标路径为空。";
                return false;
            }

            if (PathUtil.IsSubPathOf(targetFolderPath, sourceFolderPath))
            {
                error = "目标不能等于或位于源之内。";
                return false;
            }

            bool sourceManaged = isManagedArea(sourceFolderPath);
            bool targetManaged = isManagedArea(targetFolderPath);

            switch (direction)
            {
                case FolderTransferDirection.Import:
                    if (sourceManaged)
                    {
                        error = "不能从游戏数据目录内部导入。";
                        return false;
                    }

                    if (!targetManaged)
                    {
                        error = "导入目标必须位于游戏数据目录内。";
                        return false;
                    }

                    break;

                case FolderTransferDirection.Export:
                    if (!sourceManaged)
                    {
                        error = "只能导出游戏数据目录内的内容。";
                        return false;
                    }

                    if (targetManaged)
                    {
                        error = "不能导出到游戏数据目录内部。";
                        return false;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return true;
        }
    }
}
