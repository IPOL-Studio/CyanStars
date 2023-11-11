//TODO: Should refactor base IValueTaskSource, it just simple impl

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MunNovel.Utils
{
    public static class ValueTaskUtils
    {
        public async static ValueTask WhenAll(IList<ValueTask> tasks)
        {
            _ = tasks ?? throw new ArgumentNullException(nameof(tasks));

            if (tasks.Count == 0)
                return;

            List<Exception> exceptions = null;

            for (int i = 0; i < tasks.Count; i++)
            {
                try
                {
                    await tasks[i];
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();

                    exceptions.Add(e);
                }
            }

            if (exceptions != null)
                throw new AggregateException(exceptions);
        }

        public async static ValueTask WhenAll(params ValueTask[] tasks)
        {
             _ = tasks ?? throw new ArgumentNullException(nameof(tasks));

            if (tasks.Length == 0)
                return;

            List<Exception> exceptions = null;

            for (int i = 0; i < tasks.Length; i++)
            {
                try
                {
                    await tasks[i];
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();

                    exceptions.Add(e);
                }
            }

            if (exceptions != null)
                throw new AggregateException(exceptions);
        }
    }

}