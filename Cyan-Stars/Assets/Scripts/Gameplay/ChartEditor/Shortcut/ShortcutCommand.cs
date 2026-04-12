#nullable enable

using System;

namespace CyanStars.Gameplay.ChartEditor
{
    public class ShortcutCommand
    {
        public readonly string Id;
        public event Action? OnExecute;

        public ShortcutCommand(string id)
        {
            this.Id = id;
        }

        public ListenerDisposable RegisterListener(Action listener)
        {
            _ = listener ?? throw new ArgumentNullException(nameof(listener));
            OnExecute += listener;
            return new ListenerDisposable(this, listener);
        }

        private void RemoveListener(Action listener)
        {
            _ = listener ?? throw new ArgumentNullException(nameof(listener));
            OnExecute -= listener;
        }

        public virtual bool CanExecute() => true;

        public void Execute()
        {
            OnExecute?.Invoke();
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Executed chart editor command: {Id}");
#endif
        }

        public readonly struct ListenerDisposable
        {
            private readonly ShortcutCommand Owner;
            private readonly Action Target;

            internal ListenerDisposable(ShortcutCommand owner, Action target)
            {
                this.Owner = owner;
                this.Target = target;
            }

            public void Dispose() => Owner.RemoveListener(Target);
        }
    }
}
