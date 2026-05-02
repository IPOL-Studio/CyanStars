#nullable enable

using System.Collections.Generic;

namespace Gameplay.ChartEditor
{
    public static class ChartEditorAssetHelper
    {
        // 制谱器内需要加载的资源
        public static readonly string PosLinePath;
        public static readonly string BeatLinePath;
        public static readonly string TapNotePath;
        public static readonly string HoldNotePath;
        public static readonly string DragNotePath;
        public static readonly string ClickNotePath;
        public static readonly string BreakNotePath;

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
    }
}
