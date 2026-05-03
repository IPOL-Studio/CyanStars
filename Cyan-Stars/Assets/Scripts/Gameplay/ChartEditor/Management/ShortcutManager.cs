#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.Management
{
    /// <summary>
    /// 制谱器内快捷键管理器（单例）
    /// </summary>
    public class ShortcutManager : MonoBehaviour
    {
        // Priority 小的快捷键会被优先检测和执行
        private static readonly Comparer<ShortcutDefinition> ShortcutComparer =
            Comparer<ShortcutDefinition>.Create((a, b) => a.Entry.Priority.CompareTo(b.Entry.Priority));

        private List<ShortcutDefinition> shortcuts = null!;
        private HashSet<ShortcutCommand> executedCommand = null!;

        public void Init()
        {
            shortcuts = new List<ShortcutDefinition>();
            executedCommand = new HashSet<ShortcutCommand>();

            AddShortcut(ShortcutCommandRegistry.Undo, new ShortcutEntry(ShortcutModifiers.Ctrl, KeyCode.Z));
            AddShortcut(ShortcutCommandRegistry.Redo, new ShortcutEntry(ShortcutModifiers.Ctrl | ShortcutModifiers.Shift, KeyCode.Z));
            AddShortcut(ShortcutCommandRegistry.Redo, new ShortcutEntry(ShortcutModifiers.Ctrl, KeyCode.Y));
            AddShortcut(ShortcutCommandRegistry.Save, new ShortcutEntry(ShortcutModifiers.Ctrl, KeyCode.S));
        }

        private void AddShortcut(ShortcutCommand command, ShortcutEntry entry)
        {
            var shortcut = new ShortcutDefinition(command, entry);
            var insertIndex = shortcuts.BinarySearch(shortcut, ShortcutComparer);
            if (insertIndex < 0)
            {
                insertIndex = ~insertIndex;
            }

            shortcuts.Insert(insertIndex, shortcut);
        }

        void Update()
        {
            executedCommand.Clear();
            var pressedModifiers = GetPressedModifiers();

            for (int i = 0; i < shortcuts.Count; i++)
            {
                var s = shortcuts[i];
                if (executedCommand.Contains(s.Command) ||
                    s.Entry.Modifiers != pressedModifiers ||
                    !Input.GetKeyDown(s.Entry.Key) ||
                    !s.Command.CanExecute())
                    continue;

                s.Command.Execute();
                executedCommand.Add(s.Command);
            }
        }

        private ShortcutModifiers GetPressedModifiers()
        {
            ShortcutModifiers modifiers = ShortcutModifiers.None;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                modifiers |= ShortcutModifiers.Ctrl;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                modifiers |= ShortcutModifiers.Shift;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                modifiers |= ShortcutModifiers.Alt;
            return modifiers;
        }
    }
}
