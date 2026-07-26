using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace CyanStars.Editor
{
    internal static class CompilationToolbar
    {
        private enum AutoRefreshMode
        {
            Disabled,
            Enabled,
            EnabledOutsidePlayMode
        }

        private const string CompilationGroupElementPath = "CyanStars/Compilation Group";

        internal const string RefreshShortcutId = "Main Menu/Assets/Refresh";
        internal const string AutoRefreshModePreferenceKey = "kAutoRefreshMode";

        [MainToolbarElement(
            CompilationGroupElementPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 101)]
#pragma warning disable IDE0051
        private static IEnumerable<MainToolbarElement> CreateCompilationControls()
        {
            yield return new MainToolbarButton(new MainToolbarContent("Compile", GetCompileTooltip()), CompileScripts);

            AutoRefreshMode autoRefreshMode = GetAutoRefreshMode();
            var toggleContent = new MainToolbarContent(
                string.Empty,
                EditorGUIUtility.IconContent("d_Refresh").image as Texture2D,
                $"切换自动编译脚本, 当前模式: {GetAutoRefreshModeLabel(autoRefreshMode)}\n右键菜单可调节任意模式");
            yield return new MainToolbarToggle(
                toggleContent,
                autoRefreshMode != AutoRefreshMode.Disabled,
                SetAutoCompilationEnabled)
            {
                populateContextMenu = AddAutoRefreshModeMenu
            };

        }
#pragma warning restore IDE0051

        internal static void CompileScripts()
        {
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }

        internal static string GetCompileTooltip()
        {
            ShortcutBinding binding = ShortcutManager.instance.GetShortcutBinding(RefreshShortcutId);
            return $"Compile Scripts ({binding})";
        }

        private static void SetAutoCompilationEnabled(bool isEnabled)
        {
            SetAutoRefreshMode(isEnabled ? AutoRefreshMode.Enabled : AutoRefreshMode.Disabled);
        }

        private static AutoRefreshMode GetAutoRefreshMode()
        {
            return (AutoRefreshMode)EditorPrefs.GetInt(AutoRefreshModePreferenceKey);
        }

        private static void AddAutoRefreshModeMenu(DropdownMenu menu)
        {
            AutoRefreshMode currentMode = GetAutoRefreshMode();
            AddAutoRefreshModeMenuItem(AutoRefreshMode.Disabled);
            AddAutoRefreshModeMenuItem(AutoRefreshMode.Enabled);
            AddAutoRefreshModeMenuItem(AutoRefreshMode.EnabledOutsidePlayMode);

            void AddAutoRefreshModeMenuItem(AutoRefreshMode mode)
            {
                menu.AppendAction(
                    GetAutoRefreshModeLabel(mode),
                    action => SetAutoRefreshMode(mode),
                    mode == currentMode ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }
        }

        private static string GetAutoRefreshModeLabel(AutoRefreshMode mode)
        {
            return mode switch
            {
                AutoRefreshMode.Disabled => "Disabled",
                AutoRefreshMode.Enabled => "Enabled",
                AutoRefreshMode.EnabledOutsidePlayMode => "Enabled Outside Play Mode",
                _ => "Unknown"
            };
        }

        private static void SetAutoRefreshMode(AutoRefreshMode mode)
        {
            EditorPrefs.SetInt(AutoRefreshModePreferenceKey, (int)mode);
            MainToolbar.Refresh(CompilationGroupElementPath);
        }
    }
}