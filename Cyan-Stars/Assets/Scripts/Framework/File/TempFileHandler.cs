#nullable enable

using System;
using System.IO;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 可缓存的文件的句柄
    /// </summary>
    public class TempFileHandler : IReadonlyTempFileHandler, IDisposable
    {
        public TempFileHandlerState State { get; private set; }
        public string OriginFilePath { get; }
        public string TempFilePath { get; }

        private string? targetFilePath;

        public string? TargetFilePath
        {
            get => targetFilePath;
            set
            {
                if (string.IsNullOrEmpty(value)) // 将空字符串视为 null
                    value = null;
                if (targetFilePath == value)
                    return;
                State = TempFileHandlerState.Temped;
                targetFilePath = value;
            }
        }

        /// <summary>
        /// 缓存文件并实例化句柄
        /// </summary>
        /// <param name="originFilePath">要保存的原始文件绝对路径</param>
        /// <param name="tempFolder">缓存文件夹绝对路径</param>
        /// <param name="targetFilePath">目标文件绝对路径，后续可以修改</param>
        public TempFileHandler(string originFilePath, string tempFolder, string? targetFilePath)
        {
            OriginFilePath = originFilePath;
            TargetFilePath = targetFilePath;

            if (string.IsNullOrEmpty(originFilePath) || !System.IO.File.Exists(originFilePath))
            {
                Debug.LogWarning("给定的原始文件不存在");
                State = TempFileHandlerState.Unavailable;
                TempFilePath = "";
                return;
            }

            try
            {
                // 创建路径
                if (!Directory.Exists(tempFolder))
                    Directory.CreateDirectory(tempFolder);

                // 生成一个不重复的 7 位 GUID，然后复制文件到缓存区，缓存区文件格式为 [文件名].[7位GUID].[拓展名]
                string tempFilePath;
                while (true)
                {
                    string shortGuid = Guid.NewGuid().ToString("N")[..7];
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originFilePath);
                    string extension = Path.GetExtension(originFilePath);
                    string tempFileName = $"{fileNameWithoutExt}.{shortGuid}{extension}";
                    tempFilePath = PathUtil.Combine(tempFolder, tempFileName);

                    if (!System.IO.File.Exists(tempFilePath))
                        break;
                }

                System.IO.File.Copy(originFilePath, tempFilePath);

                State = TempFileHandlerState.Temped;
                TempFilePath = tempFilePath;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"未能成功复制文件到临时目录：{e.Message}");
                throw;
            }
        }

        public bool Save(bool releaseAfterSucceedSave = false)
        {
            if (string.IsNullOrEmpty(TempFilePath) || string.IsNullOrEmpty(TargetFilePath))
            {
                Debug.LogError("临时文件或目标文件路径为空，无法保存！");
                return false;
            }

            if (State == TempFileHandlerState.Unavailable)
            {
                Debug.LogError("句柄已卸载，无法保存！");
                return false;
            }

            // 覆盖目标文件路径
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TargetFilePath)); // 如果目标文件夹路径不存在，创建路径
                System.IO.File.Copy(TempFilePath, TargetFilePath!, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"覆盖目标文件时出错：{e.Message}");
                return false;
            }

            State = TempFileHandlerState.Saved;

            if (releaseAfterSucceedSave)
                Dispose();

            return true;
        }

        public void Dispose()
        {
            if (State == TempFileHandlerState.Unavailable)
            {
                // 已经释放，无需再次释放
                return;
            }

            if (string.IsNullOrEmpty(TempFilePath))
            {
                Debug.LogError("临时文件或目标文件路径为空，无法释放！");
                return;
            }

            try
            {
                // 删除临时文件
                System.IO.File.Delete(TempFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"删除临时文件时出错：{e.Message}");
                return;
            }

            State = TempFileHandlerState.Unavailable;
        }
    }
}
