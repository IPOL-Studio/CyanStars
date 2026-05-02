#nullable enable

using System.Collections.Generic;

namespace CyanStars.Gameplay.ChartEditor
{
    public static partial class ShortcutCommandRegistry
    {
        private static readonly Dictionary<string, ShortcutCommand> Commands =
            new Dictionary<string, ShortcutCommand>();

        public static ShortcutCommand Register(ShortcutCommand command)
        {
            _ = command ?? throw new System.ArgumentNullException(nameof(command));
            if (Commands.TryGetValue(command.Id, out var existing))
            {
                if (existing != command)
                {
                    throw new System.ArgumentException($"Command with id {command.Id} already exists.");
                }

                return existing;
            }

            Commands[command.Id] = command;
            return command;
        }

        public static bool TryGetCommand(string id, out ShortcutCommand command)
        {
            return Commands.TryGetValue(id, out command);
        }

        public static bool Remove(string id)
        {
            return Commands.Remove(id);
        }
    }

    partial class ShortcutCommandRegistry
    {
        public static readonly ShortcutCommand Undo;
        public static readonly ShortcutCommand Redo;
        public static readonly ShortcutCommand Save;

        static ShortcutCommandRegistry()
        {
            Undo = Register(new ShortcutCommand("undo"));
            Redo = Register(new ShortcutCommand("redo"));
            Save = Register(new ShortcutCommand("save"));
        }
    }
}
