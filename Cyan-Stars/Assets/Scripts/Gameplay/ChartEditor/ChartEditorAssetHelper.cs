#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gameplay.ChartEditor
{
    public static class ChartEditorAssetHelper
    {
        // 制谱器内需要加载的资源
        public const string PosLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/PosLine.prefab";
        public const string BeatLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab";
        public const string TapNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/TapNote.prefab";
        public const string HoldNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/HoldNote.prefab";
        public const string DragNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/DragNote.prefab";
        public const string ClickNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/ClickNote.prefab";
        public const string BreakNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BreakNote.prefab";

        private static List<string>? allPaths;

        /// <summary>
        /// 通过反射将所有路径注册到 allPaths 中，并返回
        /// </summary>
        public static List<string> GetAllPaths()
        {
            allPaths ??= typeof(ChartEditorAssetHelper)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .ToList();

            return allPaths;
        }
    }
}
