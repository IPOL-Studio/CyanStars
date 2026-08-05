#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;

namespace Gameplay.ChartEditor
{
    public static class ChartEditorAssetHelper
    {
        // 制谱器内需要加载的资源
        public static readonly string PosLinePath;
        public static readonly string BeatLinePath;

        private static readonly string TapNotePath;
        private static readonly string HoldNotePath;
        private static readonly string DragNotePath;
        private static readonly string ClickNotePath;
        private static readonly string BreakNotePath;

        public static readonly List<string> AllPaths;

        static ChartEditorAssetHelper()
        {
            AllPaths = new List<string>
            {
                (PosLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/PosLine.prefab"),
                (BeatLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab"),
                (TapNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/TapNote.prefab"),
                (HoldNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/HoldNote.prefab"),
                (DragNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/DragNote.prefab"),
                (ClickNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/ClickNote.prefab"),
                (BreakNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BreakNote.prefab")
            };
        }

        /// <summary>
        /// 获取音符类型对应的音符 prefab 路径
        /// </summary>
        public static string GetNotePrefabPath(NoteType type) => type switch
        {
            NoteType.Tap => TapNotePath,
            NoteType.Drag => DragNotePath,
            NoteType.Hold => HoldNotePath,
            NoteType.Click => ClickNotePath,
            NoteType.Break => BreakNotePath,
            _ => throw new System.NotSupportedException()
        };
    }
}
