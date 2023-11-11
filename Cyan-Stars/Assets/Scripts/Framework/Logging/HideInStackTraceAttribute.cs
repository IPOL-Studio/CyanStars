using System;

namespace CyanStars.Framework.Logging
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct| AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
    public class HideInStackTraceAttribute : Attribute
    {
    }
}
