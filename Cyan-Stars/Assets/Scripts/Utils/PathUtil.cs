#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CyanStars.Utils
{
    /// <summary>
    /// 路径拼接、归一化与比较工具
    /// </summary>
    /// <remarks>
    /// <para>把路径当字符串使用（拼接、作字典键、放进集合）时应走本类：
    /// 比较前会先 <see cref="Normalize"/>，大小写按当前平台的文件系统规则。</para>
    /// <para><see cref="Combine(string, string)"/> 用于替代 <see cref="Path.Combine(string, string)"/>，
    /// 返回值统一为适用于 Unity 的正斜杠写法。</para>
    /// </remarks>
    public static class PathUtil
    {
        /// <summary>
        /// 路径比较器，供集合和字典使用
        /// </summary>
        /// <remarks>Windows 与 macOS 下不区分大小写比较，Android 下区分大小写；
        /// 用例如 <c>new HashSet&lt;string&gt;(PathUtil.PathComparer)</c></remarks>
        public static readonly IEqualityComparer<string> PathComparer = PathSemanticComparer.Instance;

        /// <summary>
        /// 按路径语义比较两个路径，允许 null 互相比较
        /// </summary>
        public static bool PathEquals(string? path1, string? path2)
        {
            return PathSemanticComparer.Instance.Equals(path1, path2);
        }

        /// <summary>
        /// 按路径语义判断 path 是否等于 root 或位于 root 之内
        /// </summary>
        /// <param name="path">待判断的绝对路径</param>
        /// <param name="root">根目录的绝对路径</param>
        /// <remarks>
        /// <para>本地路径会先折叠 "." 与 ".." 分段；无法折叠的路径（如 <c>content://</c>）按原样比较。</para>
        /// <para>调用方可使用 <see cref="Path.GetFullPath(string)"/> 获取绝对路径。</para>
        /// </remarks>
        public static bool IsSubPathOf(string? path, string? root)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root))
                return false;

            string normalizedPath = Normalize(Canonicalize(path));
            string normalizedRoot = Normalize(Canonicalize(root));

            if (string.Equals(normalizedPath, normalizedRoot, PathSemanticComparer.Comparison))
                return true;

            // root 本身以分隔符结尾时（如 "C:/"）不再补分隔符
            string prefix = normalizedRoot.EndsWith("/", StringComparison.Ordinal)
                ? normalizedRoot
                : normalizedRoot + "/";

            return normalizedPath.StartsWith(prefix, PathSemanticComparer.Comparison);
        }

        /// <summary>
        /// 取路径的最后一段（文件名或文件夹名），兼容正反斜杠
        /// </summary>
        /// <param name="path">路径，可以为相对路径或绝对路径</param>
        /// <returns>最后一段；路径为空时返回空字符串</returns>
        public static string GetName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            return Path.GetFileName(Normalize(path));
        }

        /// <summary>
        /// 拼接两个路径，并把结果归一化
        /// </summary>
        public static string Combine(string path1, string path2)
        {
            return Normalize(Path.Combine(path1, path2));
        }

        /// <summary>
        /// 拼接三个路径，并把结果归一化
        /// </summary>
        public static string Combine(string path1, string path2, string path3)
        {
            return Normalize(Path.Combine(path1, path2, path3));
        }

        /// <summary>
        /// 拼接四个路径，并把结果归一化
        /// </summary>
        public static string Combine(string path1, string path2, string path3, string path4)
        {
            return Normalize(Path.Combine(path1, path2, path3, path4));
        }

        /// <summary>
        /// 归一化路径：分隔符统一为正斜杠，若不是根路径，一并去掉末尾多余的分隔符
        /// </summary>
        [return: NotNullIfNotNull("path")]
        public static string? Normalize(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            string normalized = path.Replace('\\', '/');

            while (normalized.Length > 1 && normalized[^1] == '/' &&
                   !(normalized.Length == 3 && normalized[1] == ':'))
            {
                normalized = normalized[..^1];
            }

            return normalized;
        }


        /// <summary>
        /// 折叠本地路径里的 "." 与 ".." 分段
        /// </summary>
        /// <returns>折叠后的路径；非本地路径或无法解析时原样返回</returns>
        private static string Canonicalize(string path)
        {
            if (path.Contains("://"))
                return path;

            try
            {
                return Path.GetFullPath(path);
            }
            catch (Exception)
            {
                // 路径不合法（如 Windows 下的非法字符）时保持原样
                return path;
            }
        }


        /// <summary>
        /// 路径语义比较器，处理不同平台的正反斜杠、路径末斜杠、大小写敏感性的路径
        /// </summary>
        private sealed class PathSemanticComparer : IEqualityComparer<string>
        {
            /// <summary>
            /// 无状态，全局共用一个实例
            /// </summary>
            internal static readonly PathSemanticComparer Instance = new PathSemanticComparer();

            // 供 PathUtil.IsSubPathOf 等方法使用同一套大小写规则
            internal static readonly StringComparison Comparison = ResolveComparison();

            /// <summary>
            /// 取当前平台的大小写比较规则
            /// </summary>
            /// <remarks>Windows/macOS 不区分大小写，其他平台如 Android/Linux 则区分大小写</remarks>
            private static StringComparison ResolveComparison()
            {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return StringComparison.OrdinalIgnoreCase;
#else
                return StringComparison.Ordinal;
#endif
            }

            public bool Equals(string? x, string? y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (x == null || y == null)
                    return false;

                return string.Equals(Normalize(x), Normalize(y), Comparison);
            }

            public int GetHashCode(string obj)
            {
                string normalized = Normalize(obj);

                return Comparison == StringComparison.OrdinalIgnoreCase
                    ? StringComparer.OrdinalIgnoreCase.GetHashCode(normalized)
                    : StringComparer.Ordinal.GetHashCode(normalized);
            }
        }
    }
}
