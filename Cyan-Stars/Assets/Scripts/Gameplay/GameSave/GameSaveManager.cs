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
        // 存档文件的路径
        private static readonly string SaveFilePath =
            Path.Combine(Application.persistentDataPath, "CyanStarsGameSave.json");

        /// <summary>
        ///     保存游戏存档
        /// </summary>
        /// <param name="gameSaveData">存档数据</param>
        public static void SaveGameSave(GameSaveData gameSaveData)
        {
            try
            {
                // 计算并设置存档数据的校验值
                gameSaveData.Verification = CalculateVerification(gameSaveData);

                // 将存档数据序列化为JSON格式，并保存到文件
                string json = JsonConvert.SerializeObject(gameSaveData, Formatting.Indented);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"存档保存成功：{SaveFilePath}");
            }
            catch (Exception ex)
            {
                // 出现异常时记录错误信息
                Debug.LogError($"保存存档时发生错误：{ex.Message}");
            }
        }

        /// <summary>
        ///     尝试加载已有的存档
        /// </summary>
        /// <returns>存档数据，如果加载失败则返回null</returns>
        public static GameSaveData LoadGameSave()
        {
            // 检查存档文件是否存在
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("存档文件不存在，需要先手动创建。");
                return null;
            }

            try
            {
                // 读取存档文件并反序列化为对象
                string json = File.ReadAllText(SaveFilePath);
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
                    // 校验失败，备份存档并返回null
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
            // 创建一个新的游戏存档数据，包含默认的版本号和创建时间
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

            // 将克隆后的存档数据序列化为JSON
            string json = JsonConvert.SerializeObject(clonedGameSaveData);

            // 使用SHA256算法计算校验值（哈希值）
            using SHA256 sha256 = SHA256.Create();

            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                StringBuilder hashString = new StringBuilder();

                // 将哈希值转换为16进制字符串
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
            // 获取存档文件所在目录
            string saveDirectory = Path.GetDirectoryName(SaveFilePath);
            if (saveDirectory == null)
            {
                throw new ArgumentNullException(nameof(SaveFilePath), "存档文件路径无效，无法获取目录。");
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

            // 重命名原存档文件进行备份
            File.Move(SaveFilePath, backupFilePath);
            Debug.Log($"存档已备份，备份文件名为：{backupFileName}");
        }
    }
}
