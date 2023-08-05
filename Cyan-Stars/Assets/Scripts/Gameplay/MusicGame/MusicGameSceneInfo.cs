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
        public readonly MusicGameSceneUIInfoCollection UIInfos;

        public MusicGameSceneInfo(string sceneName, string scenePath, MusicGameSceneUIInfoCollection uiInfos)
        {
            SceneName = sceneName;
            ScenePath = scenePath;
            UIInfos = uiInfos ?? MusicGameSceneUIInfoCollection.Empty;
        }
    }

    public sealed class MusicGameSceneUIInfoCollection : IReadOnlyCollection<IMusicGameSceneUIPanelInfo>
    {
        public static readonly MusicGameSceneUIInfoCollection Empty = new MusicGameSceneUIInfoCollection();

        private List<IMusicGameSceneUIPanelInfo> uiPanels = new List<IMusicGameSceneUIPanelInfo>();

        public int Count => uiPanels.Count;

        public IEnumerator<IMusicGameSceneUIPanelInfo> GetEnumerator() => uiPanels.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public MusicGameSceneUIInfoCollection Register<T>() where T : BaseUIPanel
        {
            uiPanels.Add(new MusicGameSceneUIPanelInfo<T>());
            return this;
        }
    }

    public interface IMusicGameSceneUIPanelInfo
    {
        public Type PanelType { get; }
        public void Open(UIManager manager, Action<BaseUIPanel> callback);
        public void Close(UIManager manager);
    }

    internal sealed class MusicGameSceneUIPanelInfo<T> : IMusicGameSceneUIPanelInfo where T : BaseUIPanel
    {
        public Type PanelType { get; } = typeof(T);

        public void Open(UIManager manager, Action<BaseUIPanel> callback)
        {
            manager.OpenUIPanel<T>(callback);
        }

        public void Close(UIManager manager)
        {
            manager.CloseUIPanel<T>();
        }
    }
}
