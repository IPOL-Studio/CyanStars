#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CyanStars.Chart.Tracks
{
    /// <summary>
    /// 轨道类型注册表 — 管理 特效轨道键 -> 轨道数据类型 的映射
    /// </summary>
    public static class TrackTypeRegistry
    {
        private static readonly Dictionary<string, Type> TrackKeyToTypeMap = new();
        private static bool initialized;

        public static void Initialize()
        {
            if (initialized)
                return;
            initialized = true;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && typeof(IChartTrackData).IsAssignableFrom(type))
                    {
                        var customAttributes = type.GetCustomAttributes<ChartTrackAttribute>(false);

                        foreach (var attribute in customAttributes)
                        {
                            TrackKeyToTypeMap.Add(attribute.TrackKey, type);
                        }
                    }
                }
            }
        }

        public static bool TryGetChartTrackType(string key, out Type type)
        {
            return TrackKeyToTypeMap.TryGetValue(key, out type);
        }
    }
}
