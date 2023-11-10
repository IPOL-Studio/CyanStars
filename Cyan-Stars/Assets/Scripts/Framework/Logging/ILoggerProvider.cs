using System;

namespace CyanStars.Framework.Logging
{
    public interface ILoggerProvider : IDisposable
    {
        public ICysLogger CreateLogger(string categoryName);
    }
}
