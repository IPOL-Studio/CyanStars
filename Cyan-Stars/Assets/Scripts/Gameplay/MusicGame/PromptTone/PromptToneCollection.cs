using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CyanStars.Framework.Logging;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public sealed class PromptToneCollection : IReadOnlyDictionary<string, AudioClip>
    {
        private Dictionary<string, AudioClip> promptTones;

        public AudioClip this[string key] => promptTones[key];

        public IEnumerable<string> Keys => promptTones.Keys;

        public IEnumerable<AudioClip> Values => promptTones.Values;

        public int Count => promptTones.Count;

        public PromptToneCollection(IReadOnlyCollection<KeyValuePair<string, AudioClip>> builtinPromptTones, ICysLogger logger = null) :
            this(builtinPromptTones, null, logger)
        {
        }

        public PromptToneCollection(IReadOnlyCollection<KeyValuePair<string, AudioClip>> builtinPromptTones,
                                    IReadOnlyCollection<KeyValuePair<string, AudioClip>> chartPromptTones,
                                    ICysLogger logger = null)
        {
            int chartPromptToneCount = chartPromptTones?.Count ?? 0;
            promptTones = new Dictionary<string, AudioClip>(builtinPromptTones.Count + chartPromptToneCount);

            AddRange(builtinPromptTones, "builtin", logger);
            AddRange(chartPromptTones, "chart", logger);
        }

        private void AddRange(IReadOnlyCollection<KeyValuePair<string, AudioClip>> promptToneCollection, string collectionName, ICysLogger logger)
        {
            if (promptToneCollection == null || promptToneCollection.Count == 0)
                return;

            foreach (var (name, clip) in promptToneCollection)
            {
                if (!ValidPromptToneName(name))
                    continue;

                if (promptTones.ContainsKey(name))
                    CysLogHelper.LogWarning($"prompt tone \"{name}\" replaced by {collectionName}", logger);

                promptTones[name] = clip;
            }
        }

        public bool ContainsKey(string key) => promptTones.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, AudioClip>> GetEnumerator() => promptTones.GetEnumerator();

        public bool TryGetValue(string key, out AudioClip value) => promptTones.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => promptTones.GetEnumerator();

        private static bool ValidPromptToneName(string name) => !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(name);
    }

    public static class PromptToneCollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue(this PromptToneCollection self, PromptToneType promptToneType, out AudioClip clip) =>
            self.TryGetValue(promptToneType.ToBuiltInPromptToneName(), out clip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AudioClip GetValueOr(this PromptToneCollection self, PromptToneType promptToneType, AudioClip other) =>
            self.TryGetValue(promptToneType.ToBuiltInPromptToneName(), out var clip) ? clip : other;
    }
}
