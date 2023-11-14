using System;
using System.Diagnostics;
using System.Text;

namespace CyanStars.Framework.Logging
{
    internal struct UnityDebugExceptionFormatter
    {
        private const int IndentCount = 4;
        private const int MaxLevel = 3;

        private readonly Exception Exception;
        private readonly StringBuilder Builder;
        private readonly bool IsTrace;

        private string cachedResult;
        private bool isFilled;

        public UnityDebugExceptionFormatter(Exception exception, StringBuilder builder, bool isTrace) : this()
        {
            this.Exception = exception;
            this.Builder = builder;
            this.IsTrace = isTrace;
        }

        public void FillString()
        {
            if (isFilled)
                return;

            AppendException(Exception, 0);
            isFilled = true;
        }

        public override string ToString()
        {
            if (Exception is null)
                return string.Empty;

            if (this.cachedResult != null)
                return this.cachedResult;

            FillString();
            cachedResult = Builder.ToString();
            return cachedResult;
        }

        private void AppendException(Exception exception, int level)
        {
            if (exception is null || level >= MaxLevel)
                return;

            if (level > 0)
            {
                Builder.Append('\n');

                // 被嵌套缩进的 exception 使用 "--->" 来做标记与分割
                Builder.Append('-', level * IndentCount - 1).Append('>');
            }

            Builder.Append(exception.GetType().Name)
                   .Append(": ")
                   .Append(exception.Message)
                   .Append('\n');

            AppendStackTrace(exception, level);

            if (exception is AggregateException aggregateException)
            {
                for (int i = 0; i < aggregateException.InnerExceptions.Count; i++)
                {
                    AppendException(aggregateException.InnerExceptions[i], level + 1);
                }
            }
            else
            {
                AppendException(exception.InnerException, level + 1);
            }
        }

        private void AppendStackTrace(Exception exception, int level)
        {
            if (exception is null || !IsTrace)
                return;

            StackTraceHelper.AppendStackTraceString(
                new StackTrace(exception, true),
                Builder,
                isAppendFirstFrameFilePath: true,
                indent: level * IndentCount * 2  // Unity console 的空格比其他字符要窄一半，使用两倍的空格来对齐字符
            );
        }
    }
}
