using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using CyanStars.Framework.Utils;
using UnityEngine;

namespace CyanStars.Framework.Logging
{
    // 尽量还原 Unity 风格的 StackTrace 格式
    internal static class StackTraceHelper
    {
        private const HideStackTraceFlags DefaultHideFlags = HideStackTraceFlags.AggressiveInlining | HideStackTraceFlags.HideInStackTrace | HideStackTraceFlags.NotFoundFile;
        private static readonly IReadOnlyDictionary<Type, string> ParameterNames;

        static StackTraceHelper()
        {
            ParameterNames = new Dictionary<Type, string>
            {
                { typeof(char),    "char" },
                { typeof(bool),    "bool" },
                { typeof(byte),    "byte" },
                { typeof(sbyte),   "sbyte" },
                { typeof(short),   "short" },
                { typeof(ushort),  "ushort" },
                { typeof(int),     "int" },
                { typeof(uint),    "uint" },
                { typeof(long),    "long" },
                { typeof(ulong),   "ulong" },
                { typeof(float),   "float" },
                { typeof(double),  "double" },
                { typeof(decimal), "decimal" },
                { typeof(object),  "object" },
                { typeof(string),  "string" },
            };
        }

        public static string GetStackTraceString(StackTrace stackTrace, bool isAppendFirstFrameFilePath = false, int indent = 0) =>
            GetStackTraceString(stackTrace, DefaultHideFlags, new StackTraceResolver(isAppendFirstFrameFilePath, indent).ResolveFrame);

        public static string GetStackTraceString(StackTrace stackTrace, HideStackTraceFlags hideFlags, bool isAppendFirstFrameFilePath = false, int indent = 0) =>
            GetStackTraceString(stackTrace, hideFlags, new StackTraceResolver(isAppendFirstFrameFilePath, indent).ResolveFrame);

        public static string GetStackTraceString(StackTrace stackTrace, HideStackTraceFlags hideFlags, Action<StackFrame, StringBuilder> resolveFrameAction)
        {
            _ = stackTrace ?? throw new ArgumentNullException(nameof(stackTrace));
            _ = resolveFrameAction ?? throw new ArgumentNullException(nameof(resolveFrameAction));

            var sb = StringBuilderCache.Acquire();
            try
            {
                AppendStackTraceString_Internal(stackTrace, hideFlags, sb, resolveFrameAction);
            }
            catch (Exception e)
            {
                StringBuilderCache.Release(sb);
                throw e;
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public static void AppendStackTraceString(StackTrace stackTrace, StringBuilder sb, bool isAppendFirstFrameFilePath = false, int indent = 0) =>
            AppendStackTraceString(stackTrace, sb, DefaultHideFlags, new StackTraceResolver(isAppendFirstFrameFilePath, indent).ResolveFrame);
        public static void AppendStackTraceString(StackTrace stackTrace, StringBuilder sb, HideStackTraceFlags hideFlags, bool isAppendFirstFrameFilePath = false, int indent = 0) =>
            AppendStackTraceString(stackTrace, sb, hideFlags, new StackTraceResolver(isAppendFirstFrameFilePath, indent).ResolveFrame);

        public static void AppendStackTraceString(StackTrace stackTrace, StringBuilder sb, HideStackTraceFlags hideFlags, Action<StackFrame, StringBuilder> resolveFrameAction)
        {
            _ = stackTrace ?? throw new ArgumentNullException(nameof(stackTrace));
            _ = sb ?? throw new ArgumentNullException(nameof(sb));
            _ = resolveFrameAction ?? throw new ArgumentNullException(nameof(resolveFrameAction));

            AppendStackTraceString_Internal(stackTrace, hideFlags, sb, resolveFrameAction);
        }

        private static void AppendStackTraceString_Internal(StackTrace stackTrace, HideStackTraceFlags hideFlags, StringBuilder sb, Action<StackFrame, StringBuilder> resolveFrameAction)
        {
            var frames = stackTrace.GetFrames();

            for (int i = 0; i < frames.Length; i++)
            {
                if (IsSkipFrame(frames[i], hideFlags))
                {
                    continue;
                }

                resolveFrameAction(frames[i], sb);
            }

            int lastIndex = sb.Length - 1;
            if (lastIndex >= 0 && sb[lastIndex] == '\n')
            {
                sb.Remove(lastIndex, 1);
            }
        }

        private static bool IsSkipFrame(StackFrame frame, HideStackTraceFlags hideFlags)
        {
            if (hideFlags == HideStackTraceFlags.None)
                return false;

            if (hideFlags == (HideStackTraceFlags.AggressiveInlining | HideStackTraceFlags.HideInStackTrace | HideStackTraceFlags.NotFoundFile | HideStackTraceFlags.FoundFile))
                return true;

            var mb = frame.GetMethod();

            if (hideFlags.HasFlagValue(HideStackTraceFlags.AggressiveInlining) && (mb.MethodImplementationFlags & MethodImplAttributes.AggressiveInlining) != 0)
            {
                // don't show when method is AggressiveInlining
                return true;
            }

            if (hideFlags.HasFlagValue(HideStackTraceFlags.HideInStackTrace) && IsHideInStack(mb))
            {
                // don't show when method is marked with HideInStackTraceAttribute or other similar attribute
                return true;
            }

            var fileName = frame.GetFileName();

            if (hideFlags.HasFlagValue(HideStackTraceFlags.NotFoundFile) && fileName == null)
            {
                // don't show when file is not found
                return true;
            }

            if (hideFlags.HasFlagValue(HideStackTraceFlags.FoundFile) && fileName != null)
            {
                // don't show when file is found
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsHideInStack(MethodBase mb)
        {
            return mb.IsDefined(typeof(HideInStackTraceAttribute), inherit: false)
                || mb.DeclaringType.IsDefined(typeof(HideInStackTraceAttribute), inherit: false)
#if NET6_0_OR_GREATER
                || mb.IsDefined(typeof(System.Diagnostics.StackTraceHiddenAttribute), inherit: false)
                || mb.DeclaringType.IsDefined(typeof(System.Diagnostics.StackTraceHiddenAttribute), inherit: false)
#endif
#if UNITY_2022_2_OR_NEWER
                || mb.IsDefined(typeof(UnityEngine.HideInCallstackAttribute), inherit: false)  // it just AttributeTargets.Method
#endif
                ;
        }

        private static string GetParameterTypeString(Type type)
        {
            return ParameterNames.TryGetValue(type, out string name) ? name : type.ToString();
        }

        private static void AppendParameter(StringBuilder sb, ParameterInfo parameter)
        {
            var type = parameter.ParameterType;

            if (type.IsByRef)
            {
                if (parameter.IsIn)
                {
                    sb.Append("in ");
                }
                else if (parameter.IsOut)
                {
                    sb.Append("out ");
                }
                else
                {
                    sb.Append("ref ");
                }

                type = type.GetElementType();  // type& -> type
            }

            if (type.IsArray)
            {
                int count = 0;
                while (type.IsArray)
                {
                    type = type.GetElementType();  // type[] -> type
                    count++;
                }
                sb.Append(GetParameterTypeString(type));
                while (count-- > 0)
                {
                    sb.Append("[]");
                }
            }
            else
            {
                // TODO: 泛型输出，目前走 CSharp 的 ToString() 输出
                // 与 Unity Debug 的输出不太一样

                sb.Append(GetParameterTypeString(type));
            }
        }

        internal static void AppendMethodDesc(StringBuilder sb, MethodBase method)
        {
            sb.Append(method.Name);

            sb.Append('(');
            string s = string.Empty;

            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                sb.Append(s);
                AppendParameter(sb, parameters[i]);
                s = ", ";
            }

            sb.Append(')');
        }

        internal static void AppendFileLink(StringBuilder sb, string fileName, int fileNameStart, int fileNameLength, int lineNumber)
        {
            sb.Append("<a href=").Append('"').Append(fileName, fileNameStart, fileNameLength).Append('"')
              .Append(" line=").Append('"').Append(lineNumber).Append('"')
              .Append('>')
              .Append(fileName, fileNameStart, fileNameLength).Append(':').Append(lineNumber)
              .Append("</a>");
        }

        internal static bool TryConvertFileNameToUnityAssetsFormat(string fileName, out int startIndex, out int length)
        {
            startIndex = -1;
            length = -1;

            if (!Application.isEditor || fileName.Length < 8)
                return false;

            var index = fileName.IndexOf("Assets/", StringComparison.Ordinal);
            if (index <= 0)
            {
                index = fileName.IndexOf("Library/PackageCache/", StringComparison.Ordinal);

                if (index <= 0)
                    return false;
            }

            startIndex = index;
            length = fileName.Length - index;
            return true;
        }

        private struct StackTraceResolver
        {
            private bool isAppendFrameFileLink;
            private int indent;

            public StackTraceResolver(bool isAppendFirstFrameFilePath, int indent)
            {
                this.isAppendFrameFileLink = isAppendFirstFrameFilePath;
                this.indent = indent > 0 ? indent : 0;
            }

            public void ResolveFrame(StackFrame frame, StringBuilder sb)
            {
                sb.Append(' ', indent)
                  .Append(frame.GetMethod().DeclaringType.ToString())
                  .Append(':');

                AppendMethodDesc(sb, frame.GetMethod());

                if (isAppendFrameFileLink && 
                    TryResolveFrameFileLink(frame, out string fileName, out int fileNameStart, out int fileNameLength))
                {
                    sb.Append(" (at ");
                    AppendFileLink(sb, fileName, fileNameStart, fileNameLength, frame.GetFileLineNumber());
                    sb.Append(')');
                }

                sb.Append('\n');

                isAppendFrameFileLink = true;
            }

            private static bool TryResolveFrameFileLink(StackFrame frame, out string fileName, out int startIndex, out int length)
            {
                fileName = frame.GetFileName();
                startIndex = -1;
                length = -1;

                if (!string.IsNullOrEmpty(fileName))
                {
                    fileName = fileName.Replace('\\', '/');
                    return TryConvertFileNameToUnityAssetsFormat(fileName, out startIndex, out length);
                }

                return false;
            }
        }
    }
}
