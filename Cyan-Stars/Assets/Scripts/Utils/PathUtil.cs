#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

namespace CyanStars.Utils
{
    /// <summary>
    /// 路径拼接、归一化与比较工具
    /// </summary>
    /// <remarks>
    /// <para>把路径当字符串使用（拼接、作字典键、放进集合）时应走本类：比较前会先 <see cref="Normalize"/>，大小写按当前平台的文件系统规则。</para>
    /// <para><see cref="Combine(string, string)"/> 用于替代 <see cref="Path.Combine(string, string)"/>，返回值统一为正斜杠写法。</para>
    /// </remarks>
    public static class PathUtil
    {
        /// <summary>
        /// 路径比较器，供集合和字典使用
        /// </summary>
        /// <remarks>Windows 与 macOS 下不区分大小写比较，Android 下区分大小写；
        /// 用例如 <c>new HashSet&lt;string&gt;(PathUtil.PathComparer)</c></remarks>
        public static IEqualityComparer<string> PathComparer { get; } = PathSemanticComparer.Instance;

        /// <summary>
        /// 按路径语义比较两个路径，允许 null
        /// </summary>
        /// <remarks>两个 null 视为相同；<see cref="IEqualityComparer{T}.Equals"/> 的形参声明为非 null，直接传可空实参会报 CS8604 警告</remarks>
        public static bool PathEquals(string? path1, string? path2)
        {
            return PathSemanticComparer.Instance.Equals(path1, path2);
        }

        /// <summary>
        /// 判断 path 是否等于 root 或位于 root 之内，按路径语义比较
        /// </summary>
        /// <param name="path">待判断的绝对路径</param>
        /// <param name="root">根目录的绝对路径</param>
        /// <remarks>比较前双方都会先过一遍 <see cref="Normalize"/>，调用方需传入绝对路径
        /// （必要时先 <see cref="Path.GetFullPath(string)"/> 消掉相对路径和 <c>..</c>）。
        /// 前缀比较带分隔符边界，<c>C:/data</c> 不会匹配 <c>C:/database</c>。</remarks>
        public static bool IsSubPathOf(string path, string root)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root))
                return false;

            string normalizedPath = Normalize(path);
            string normalizedRoot = Normalize(root);

            if (string.Equals(normalizedPath, normalizedRoot, PathSemanticComparer.Comparison))
                return true;

            // root 本身以分隔符结尾时（如 "C:/"）不再补分隔符
            string prefix = normalizedRoot.EndsWith("/", StringComparison.Ordinal)
                ? normalizedRoot
                : normalizedRoot + "/";

            return normalizedPath.StartsWith(prefix, PathSemanticComparer.Comparison);
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
        /// 归一化路径：分隔符统一为正斜杠，去掉末尾多余的分隔符
        /// </summary>
        /// <remarks>不做大小写归一化，大小写规则由调用方处理</remarks>
        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            string normalized = path.Replace('\\', '/');

            // 保留 "C:/"、"/" 这类根路径
            while (normalized.Length > 1 && normalized[normalized.Length - 1] == '/' &&
                   !(normalized.Length == 3 && normalized[1] == ':'))
            {
                normalized = normalized[..^1];
            }

            return normalized;
        }


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
            private static StringComparison ResolveComparison()
            {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return StringComparison.OrdinalIgnoreCase;
#else
                // Android 区分大小写；编辑器可能运行在 Windows / macOS 上
                return UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor ||
                       UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXEditor
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;
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
