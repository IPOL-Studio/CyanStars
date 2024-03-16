using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework.UI;

namespace CyanStars.Gameplay.MusicGame
{
    public sealed class MusicGameSceneInfo
    {
        public readonly string SceneName;
        public readonly string ScenePath;
        public readonly MusicGameSceneUICollection UICollection;

        public MusicGameSceneInfo(string sceneName, string scenePath, MusicGameSceneUICollection uiCollection)
        {
            SceneName = sceneName;
            ScenePath = scenePath;
            UICollection = uiCollection ?? MusicGameSceneUICollection.Empty;
        }
    }

    public sealed class MusicGameSceneUICollection : IReadOnlyCollection<Type>
    {
        public static readonly MusicGameSceneUICollection Empty = new MusicGameSceneUICollection();

        private List<Type> uiPanels = new List<Type>();

        public int Count => uiPanels.Count;

        public IEnumerator<Type> GetEnumerator() => uiPanels.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public MusicGameSceneUICollection Register<T>() where T : BaseUIPanel
        {
            uiPanels.Add(typeof(T));
            return this;
        }
    }
}
