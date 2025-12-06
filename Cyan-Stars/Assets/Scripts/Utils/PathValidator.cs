using System.Collections.Generic;

namespace CyanStars.Utils
{
    /// <summary>
    /// 验证文件夹名称是否包含非法字符
    /// </summary>
    public static class PathValidator
    {
        private static readonly char[] InvalidChars = new char[] { '\"', '<', '>', '|', ':', '*', '?', '\\', '/' };

        private static readonly HashSet<string> ReservedNames =
            new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            {
                "CON",
                "PRN",
                "AUX",
                "NUL",
                "COM1",
                "COM2",
                "COM3",
                "COM4",
                "COM5",
                "COM6",
                "COM7",
                "COM8",
                "COM9",
                "LPT1",
                "LPT2",
                "LPT3",
                "LPT4",
                "LPT5",
                "LPT6",
                "LPT7",
                "LPT8",
                "LPT9"
            };

        private static readonly string ContainsInvalidCharsMessage = $"文件夹名不能包含以下字符: {string.Join(" ", InvalidChars)}";


        public static bool IsValidFolderName(string name, out string errorMessage, int maxLength = 240)
        {
            // 检查是否为空或仅包含空白字符
            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "文件夹名不能为空。";
                return false;
            }

            // 检查长度
            if (name.Length > maxLength)
            {
                errorMessage = $"文件夹名长度不能超过 {maxLength} 个字符。";
                return false;
            }

            // 检查是否以空格或句点结尾
            if (name.EndsWith(" ") || name.EndsWith("."))
            {
                errorMessage = "文件夹名不能以空格或句点结尾。";
                return false;
            }

            // 检查是否包含非法字符
            if (name.IndexOfAny(InvalidChars) >= 0)
            {
                errorMessage = ContainsInvalidCharsMessage;
                return false;
            }

            // 检查是否为系统保留字
            if (ReservedNames.Contains(name))
            {
                errorMessage = "文件夹名不能使用系统保留字。";
                return false;
            }

            // 所有检查通过
            errorMessage = string.Empty;
            return true;
        }
    }
}
