using System.Collections.Generic;
using CyanStars.Framework;

namespace CyanStars.Gameplay.MusicGame
{
    public class MusicGameSceneModule : BaseDataModule
    {
        private List<MusicGameSceneInfo> scenes = new List<MusicGameSceneInfo>();
        private MusicGameSceneInfo currentScene;

        public MusicGameSceneInfo Fallback { get; private set; }
        public IReadOnlyList<MusicGameSceneInfo> RegisteredScenes => scenes;

        public MusicGameSceneInfo CurrentScene
        {
            get => currentScene ?? Fallback;
            set => currentScene = value;
        }

        public override void OnInit()
        {
            var uiCollection = new MusicGameSceneUICollection()
                .Register<MusicGameMainPanel>()
                .Register<MusicGame3DUIPanel>();

            scenes.Add(new MusicGameSceneInfo(
                "Warm",
                "Assets/BundleRes/Scenes/Warm.unity",
                uiCollection));

            scenes.Add(new MusicGameSceneInfo(
                "Dark",
                "Assets/BundleRes/Scenes/Dark.unity",
                uiCollection
            ));

            Fallback = scenes[0];
            currentScene = Fallback;
        }
    }
}
