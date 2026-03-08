using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CyanStars.Chart;
using CyanStars.Framework;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    public enum ChartEditorAssetType
    {
        PosLine,
        BeatLine,
        TapNote,
        DragNote,
        HoldNote,
        ClickNote,
        BreakNote
    }

    public class ChartEditorAssetManager : MonoBehaviour
    {
        private static readonly Dictionary<ChartEditorAssetType, string> PathsDict = new()
        {
            { ChartEditorAssetType.PosLine, "Assets/BundleRes/Prefabs/ChartEditor/EditArea/PosLine.prefab" },
            { ChartEditorAssetType.BeatLine, "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab" },
            { ChartEditorAssetType.TapNote, "Assets/BundleRes/Prefabs/ChartEditor/EditArea/TapNote.prefab" },
            { ChartEditorAssetType.DragNote, "Assets/BundleRes/Prefabs/ChartEditor/EditArea/DragNote.prefab" },
            { ChartEditorAssetType.HoldNote, "Assets/BundleRes/Prefabs/ChartEditor/EditArea/HoldNote.prefab" },
            { ChartEditorAssetType.ClickNote, "Assets/BundleRes/Prefabs/ChartEditor/EditArea/ClickNote.prefab" },
            { ChartEditorAssetType.BreakNote, "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BreakNote.prefab" }
        };


        public async Task Init()
        {
            // 预加载资源，防止在制谱器播放时加载造成奇怪的 bug
            // TODO: 具体排查 bug：不预热在播放时加载出现的偶发加载失败
            List<string> pathsToInit = new(PathsDict.Values);
            _ = await GameRoot.Asset.BatchLoadAssetAsync(pathsToInit).AddTo(this);
        }


        public async Task<GameObject> GetGameObjectAsync(NoteType type, Transform parent, CancellationToken token = default)
        {
            return await GetGameObjectAsync(GetNoteAssetType(type), parent, token);
        }

        public async Task<GameObject> GetGameObjectAsync(ChartEditorAssetType type, Transform parent, CancellationToken token = default)
        {
            return await GameRoot.GameObjectPool.GetGameObjectAsync(GetPathByType(type), parent, token);
        }


        public void ReleaseGameObject(NoteType type, GameObject go)
        {
            ReleaseGameObject(GetNoteAssetType(type), go);
        }

        public void ReleaseGameObject(ChartEditorAssetType type, GameObject go)
        {
            GameRoot.GameObjectPool.ReleaseGameObject(GetPathByType(type), go);
        }


        private static ChartEditorAssetType GetNoteAssetType(NoteType type) => type switch
        {
            NoteType.Tap => ChartEditorAssetType.TapNote,
            NoteType.Drag => ChartEditorAssetType.DragNote,
            NoteType.Hold => ChartEditorAssetType.HoldNote,
            NoteType.Click => ChartEditorAssetType.ClickNote,
            NoteType.Break => ChartEditorAssetType.BreakNote,
            _ => throw new NotSupportedException()
        };

        private static string GetPathByType(ChartEditorAssetType type)
        {
            return PathsDict[type];
        }
    }
}
