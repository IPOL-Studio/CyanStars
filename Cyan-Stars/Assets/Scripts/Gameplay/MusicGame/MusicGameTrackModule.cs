using System;
using System.Collections.Generic;
using System.Reflection;
using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class MusicGameTrackModule : BaseDataModule
    {
        private Dictionary<Type, ITrackLoader> chartTrackTypeLoader = new Dictionary<Type, ITrackLoader>();

        public override void OnInit()
        {
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
                    this.chartTrackTypeLoader.Add(attr.ChartTrackDataType, trackLoader);
                }
            }

            Debug.Log($"TrackLoader count: {this.chartTrackTypeLoader.Count}");
        }

        public bool TryGetTrackLoader(Type chartTrackType, out ITrackLoader trackLoader)
            => this.chartTrackTypeLoader.TryGetValue(chartTrackType, out trackLoader);
    }
}
