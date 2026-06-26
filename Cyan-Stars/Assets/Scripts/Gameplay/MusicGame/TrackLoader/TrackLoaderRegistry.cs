#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 轨道加载器注册表 — 管理 轨道数据类型 -> ITrackLoader 的映射
    /// </summary>
    public static class TrackLoaderRegistry
    {
        private static readonly Dictionary<Type, ITrackLoader> ChartTrackTypeToLoader = new();
        private static bool initialized;

        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!typeof(ITrackLoader).IsAssignableFrom(type) ||
                        type.IsAbstract ||
                        type.GetConstructor(Type.EmptyTypes) is null)
                    {
                        continue;
                    }

                    var attr = type.GetCustomAttribute<TrackLoaderAttribute>();
                    if (attr?.ChartTrackDataType is null)
                        continue;

                    var trackLoader = (ITrackLoader)Activator.CreateInstance(type);
                    ChartTrackTypeToLoader.Add(attr.ChartTrackDataType, trackLoader);
                }
            }
        }

        public static bool TryGetTrackLoader(Type chartTrackType, out ITrackLoader trackLoader)
        {
            return ChartTrackTypeToLoader.TryGetValue(chartTrackType, out trackLoader);
        }
    }
}
