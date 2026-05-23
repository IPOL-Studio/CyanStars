#nullable enable

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 用于将 ChartPackInfo 解析为 TMP Text 或 Staffs 的静态工具类
    /// </summary>
    public static class ChartPackInfoHelper
    {
        private static readonly Color AtColor = new(1, 0.841f, 0.078f, 0.87f);
        private static readonly string AtHexColor = ColorUtility.ToHtmlStringRGBA(AtColor);
        private static readonly Regex AtRegex = new(@"\[@([^\]]+)\]", RegexOptions.Compiled); // [@User Name]

        /// <summary>
        /// 解析为带格式的 TMP Text
        /// </summary>
        public static string ToTmpText(string chartPackInfo)
        {
            if (string.IsNullOrEmpty(chartPackInfo))
                return string.Empty;

            string replacement = $"<color=#{AtHexColor}>@$1</color>";
            chartPackInfo = AtRegex.Replace(chartPackInfo, replacement);

            // TODO: 进行 Markdown 格式解析

            return chartPackInfo;
        }

        /// <summary>
        /// 解析 Staffs 并去重
        /// </summary>
        public static HashSet<string> ToStaffs(string chartPackInfo)
        {
            HashSet<string> staffSet = new();

            if (string.IsNullOrEmpty(chartPackInfo))
                return staffSet;

            MatchCollection matches = AtRegex.Matches(chartPackInfo);
            if (matches.Count == 0)
                return staffSet;

            foreach (Match match in matches)
            {
                if (match.Groups.Count <= 1)
                    continue;

                string staffName = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(staffName))
                {
                    staffSet.Add(staffName);
                }
            }

            return staffSet;
        }
    }
}
