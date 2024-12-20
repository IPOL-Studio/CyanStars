using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace CyanStars.Gameplay.GameSave
{
    /// <summary>
    ///     管理游戏存档的保存和加载操作
    /// </summary>
    public static class GameSaveManager
    {
        /// <summary>
        /// 获取此设备上的存档文件路径
        /// </summary>
        /// <returns>存档文件的路径</returns>
        public static string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, "CyanStarsGameSave.json");
        }

        /// <summary>
        ///     保存游戏存档
        /// </summary>
        /// <param name="gameSaveData">存档数据</param>
        public static void SaveGameSave(GameSaveData gameSaveData)
        {
            string saveFilePath = GetSaveFilePath();
            try
            {
                gameSaveData.Verification = CalculateVerification(gameSaveData);

                string json = JsonConvert.SerializeObject(gameSaveData, Formatting.Indented);
                File.WriteAllText(saveFilePath, json);
                Debug.Log($"存档保存成功：{saveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"保存存档时发生错误：{ex.Message}");
            }
        }

        /// <summary>
        ///     尝试加载已有的存档
        /// </summary>
        /// <returns>存档数据，如果加载失败则返回null</returns>
        public static GameSaveData LoadGameSave()
        {
            string saveFilePath = GetSaveFilePath();
            if (!File.Exists(saveFilePath))
            {
                Debug.LogWarning("存档文件不存在，需要先手动创建。");
                return null;
            }

            try
            {
                string json = File.ReadAllText(saveFilePath);
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None
                };
                GameSaveData gameSaveData = JsonConvert.DeserializeObject<GameSaveData>(json);
                Debug.Log("存档读取成功。");

                // 计算存档数据的校验值并验证
                string verification = CalculateVerification(gameSaveData);
                if (verification != gameSaveData.Verification)
                {
                    Debug.LogWarning("存档校验失败，存档可能被篡改或版本不匹配。");
                    BackupSaveFile();
                    return null;
                }

                Debug.Log("存档校验通过。");
                return gameSaveData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载存档时发生错误：{ex.Message}");
                BackupSaveFile();
                return null;
            }
        }

        /// <summary>
        ///     创建一个默认的游戏存档数据，不会自动保存
        /// </summary>
        /// <returns>默认的游戏存档数据</returns>
        public static GameSaveData CreateDefaultGameSave()
        {
            return new GameSaveData
            {
                Version = 1,
                CreateTime = DateTime.Now,
                SaveTime = DateTime.Now,
                Verification = null, // 校验码暂时为空
                MusicGameData = new GameSaveData.MusicGameSaveData()
            };
        }

        /// <summary>
        ///     计算存档数据的校验值，用于验证存档是否未被篡改
        /// </summary>
        /// <param name="gameSaveData">要计算校验值的存档数据</param>
        /// <returns>存档数据的校验值</returns>
        private static string CalculateVerification(GameSaveData gameSaveData)
        {
            // 克隆存档数据，但不包含Verification属性
            GameSaveData clonedGameSaveData = new GameSaveData
            {
                Version = gameSaveData.Version,
                CreateTime = gameSaveData.CreateTime,
                SaveTime = gameSaveData.SaveTime,
                MusicGameData = gameSaveData.MusicGameData
            };

            string json = JsonConvert.SerializeObject(clonedGameSaveData);

            using SHA256 sha256 = SHA256.Create();

            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                StringBuilder hashString = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    hashString.Append(b.ToString("x2"));
                }

                return hashString.ToString();
            }
        }

        /// <summary>
        ///     重命名原存档并备份（在存档校验失败时）
        /// </summary>
        private static void BackupSaveFile()
        {
            string saveFilePath = GetSaveFilePath();
            string saveDirectory = Path.GetDirectoryName(saveFilePath);
            if (saveDirectory == null)
            {
                throw new ArgumentNullException(nameof(saveFilePath), "存档文件路径无效，无法获取目录。");
            }

            // 查找一个可用的备份文件名
            int backupNumber = 1;
            string backupFileName = $"CyanStarsGameSaveBackup{backupNumber}.json";
            string backupFilePath = Path.Combine(saveDirectory, backupFileName);

            while (File.Exists(backupFilePath))
            {
                backupNumber++;
                backupFileName = $"CyanStarsGameSaveBackup{backupNumber}.json";
                backupFilePath = Path.Combine(saveDirectory, backupFileName);
            }

            File.Move(saveFilePath, backupFilePath);
            Debug.Log($"存档已备份，备份文件名为：{backupFileName}");
        }
    }
}
