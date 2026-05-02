#nullable enable

using System;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    public sealed record ShortcutDefinition(ShortcutCommand Command, ShortcutEntry Entry);

    [Flags]
    public enum ShortcutModifiers
    {
        None = 0,
        Ctrl = 1 << 0,
        Shift = 1 << 1,
        Alt = 1 << 2,
    }

    public sealed record ShortcutEntry
    {
        public readonly ShortcutModifiers Modifiers;
        public readonly KeyCode Key;
        public readonly int Priority;

        public ShortcutEntry(ShortcutModifiers modifiers, KeyCode key, int priority = 0)
        {
            this.Modifiers = modifiers;
            this.Key = key;
            this.Priority = priority;
        }
    }
}
