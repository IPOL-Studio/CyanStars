#nullable enable

using System;
using System.IO;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 缓存区里的一份暂存文件句柄
    /// </summary>
    /// <remarks>
    /// <para>由 <see cref="TempFileStore"/> 创建并改写，可通过 <see cref="IReadonlyStagedFileHandle"/> 只读访问</para>
    /// <para>创建后 <see cref="StagedFilePath"/> 和 <see cref="OriginFilePath"/> 不再变化</para>
    /// <para><see cref="TargetFilePath"/> 可由 <see cref="TempFileStore.Retarget"/> 改写</para>
    /// </remarks>
    internal sealed class StagedFileHandle : IReadonlyStagedFileHandle
    {
        public string StagedFilePath { get; }

        public string OriginFilePath { get; }

        public string? TargetFilePath { get; private set; }

        public StagedFileState State { get; private set; }


        /// <summary>
        /// 创建句柄并把外部文件复制到缓存区
        /// </summary>
        /// <param name="originFilePath">外部文件的绝对路径，可以是普通路径或安卓 content:// 路径</param>
        /// <param name="stagedFilePath">缓存区内的文件绝对路径；调用方需保证该路径未被占用且所在文件夹已存在</param>
        /// <exception cref="FileNotFoundException">外部文件不存在</exception>
        /// <exception cref="Exception">复制到缓存区失败</exception>
        internal StagedFileHandle(string originFilePath, string stagedFilePath)
        {
            OriginFilePath = originFilePath;
            StagedFilePath = stagedFilePath;

            if (string.IsNullOrEmpty(originFilePath) || !FileManager.IsFileExists(originFilePath))
            {
                State = StagedFileState.Released;
                Debug.LogError($"外部文件不存在：{originFilePath}");
                throw new FileNotFoundException($"外部文件不存在：{originFilePath}", originFilePath);
            }

            try
            {
                FileManager.CopyFileOrFolderUnchecked(originFilePath, stagedFilePath, false);
                State = StagedFileState.Staged;
            }
            catch (Exception e)
            {
                State = StagedFileState.Released;
                Debug.LogError($"复制文件到缓存区失败：{e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 更新目标路径
        /// </summary>
        internal void SetTargetFilePath(string targetFilePath)
        {
            if (State == StagedFileState.Released)
                return;

            TargetFilePath = targetFilePath;
        }

        /// <summary>
        /// 解除目标路径映射，暂存副本仍然保留
        /// </summary>
        internal void Detach()
        {
            if (State == StagedFileState.Released)
                return;

            TargetFilePath = null;
        }

        /// <summary>
        /// 释放暂存副本，不再使用
        /// </summary>
        internal void Release()
        {
            State = StagedFileState.Released;
            TargetFilePath = null;
        }
    }
}
