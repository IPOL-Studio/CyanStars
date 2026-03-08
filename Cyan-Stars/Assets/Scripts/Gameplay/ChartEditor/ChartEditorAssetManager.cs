using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CyanStars.Framework;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    public class ChartEditorAssetManager : MonoBehaviour
    {
        // 预制体路径
        public const string PosLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/PosLine.prefab";
        public const string BeatLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab";
        public const string TapNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/TapNote.prefab";
        public const string DragNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/DragNote.prefab";
        public const string HoldNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/HoldNote.prefab";
        public const string ClickNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/ClickNote.prefab";
        public const string BreakNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BreakNote.prefab";

        private readonly List<string> PathsToInit = new()
        {
            PosLinePath,
            BeatLinePath,
            TapNotePath,
            DragNotePath,
            HoldNotePath,
            ClickNotePath,
            BreakNotePath
        };

        public async Task Init()
        {
            // 预加载资源，防止在制谱器播放时加载造成奇怪的 bug
            // TODO: 具体排查 bug
            _ = await GameRoot.Asset.BatchLoadAssetAsync(PathsToInit).AddTo(this);
        }


        public async Task<GameObject> GetGameObjectAsync(string path, Transform parent, CancellationToken token = default)
        {
            return await GameRoot.GameObjectPool.GetGameObjectAsync(path, parent, token);
        }


        public void ReleaseGameObject(string path, GameObject go)
        {
            GameRoot.GameObjectPool.ReleaseGameObject(path, go);
        }
    }
}
