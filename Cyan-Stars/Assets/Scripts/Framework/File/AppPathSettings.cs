#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Framework.File
{
    [Serializable]
    internal sealed class AppPathSettings
    {
        /// <summary>
        /// 应用路径覆盖的可序列化 POCO，键为 <see cref="AppPathKind"/> 的名称，值为目录绝对路径
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Overrides { get; set; } = new Dictionary<string, string>();

        public string? GetOverride(AppPathKind kind)
        {
            if (Overrides.TryGetValue(kind.ToString(), out string? path) && !string.IsNullOrWhiteSpace(path))
                return path;

            return null;
        }

        public void SetOverride(AppPathKind kind, string path)
        {
            Overrides[kind.ToString()] = path;
        }

        public bool RemoveOverride(AppPathKind kind)
        {
            return Overrides.Remove(kind.ToString());
        }

        public bool HasOverride(AppPathKind kind)
        {
            return GetOverride(kind) != null;
        }
    }
}
