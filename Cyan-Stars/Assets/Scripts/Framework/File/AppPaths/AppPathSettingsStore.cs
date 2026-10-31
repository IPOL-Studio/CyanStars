#nullable enable

using System;
using System.IO;
using System.Text;
using CyanStars.Utils;
using Newtonsoft.Json;
using UnityEngine;
using IOFile = System.IO.File;

namespace CyanStars.Framework.File.AppPaths
{
    /// <summary>
    /// 应用路径配置文件的读写，配置文件固定在默认持久化数据目录下
    /// </summary>
    internal static class AppPathSettingsStore
    {
        private const string SettingsFolderName = "Settings";
        private const string SettingsFileName = "AppPaths.json";

        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);


        public static string GetSettingsFilePath(string persistentDataRoot)
        {
            return PathUtil.Combine(persistentDataRoot, SettingsFolderName, SettingsFileName);
        }

        /// <summary>
        /// 读取配置；文件缺失、内容为空或解析失败时返回空配置
        /// </summary>
        public static AppPathSettings Load(string persistentDataRoot)
        {
            string settingsFilePath = GetSettingsFilePath(persistentDataRoot);

            if (!IOFile.Exists(settingsFilePath))
                return new AppPathSettings();

            try
            {
                string json = IOFile.ReadAllText(settingsFilePath, Encoding.UTF8);
                AppPathSettings? settings = JsonConvert.DeserializeObject<AppPathSettings>(json);
                if (settings == null)
                {
                    Debug.LogWarning($"路径设置文件内容为空，已使用默认设置：{settingsFilePath}");
                    return new AppPathSettings();
                }

                return settings;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"读取路径设置失败，已使用默认设置：{settingsFilePath}\n{e.Message}");
                return new AppPathSettings();
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static bool TrySave(string persistentDataRoot, AppPathSettings settings, out string error)
        {
            error = "";

            string settingsFilePath = GetSettingsFilePath(persistentDataRoot);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath)!);

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                IOFile.WriteAllText(settingsFilePath, json, Utf8NoBom);

                return true;
            }
            catch (Exception e)
            {
                error = $"保存路径设置失败：{e.Message}";
                Debug.LogError(error);
                return false;
            }
        }
    }
}
