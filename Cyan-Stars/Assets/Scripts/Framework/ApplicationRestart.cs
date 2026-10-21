#nullable enable

using CyanStars.Framework.File;
using UnityEngine;

#if !UNITY_EDITOR
using System;
#endif


namespace CyanStars.Framework
{
    /// <summary>
    /// 重启游戏进程
    /// </summary>
    public static class ApplicationRestart
    {
        /// <summary>
        /// 退出当前进程，并在支持自动重启的平台上重新拉起游戏；编辑器下只退出播放模式
        /// </summary>
        /// <param name="environment">当前平台环境</param>
        public static void Restart(IPlatformEnvironment environment)
        {
            if (environment == null || !environment.CanRestartApplication)
            {
                Debug.LogError("当前平台不支持自动重启，请手动重新打开游戏。");
                return;
            }

#if UNITY_EDITOR
            Debug.Log("编辑器下无法自动重启，已退出播放模式，请手动重新进入播放。");
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE_WIN
            try
            {
                string? executablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(executablePath))
                {
                    Debug.LogError($"启动新的游戏进程失败，请手动重新打开游戏。");
                    return;
                }

                // 构建版本由 Unity 的「强制单实例」限制多开：当前进程尚未退出时，直接拉起的新进程会被拦截。
                // 因此交给一个独立的后台 cmd 进程延迟 1 秒，等当前进程退出、单实例锁释放后再启动新游戏。
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c timeout /t 1 /nobreak >nul & start \"\" \"{executablePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                string? workingDirectory = System.IO.Path.GetDirectoryName(executablePath);
                if (!string.IsNullOrEmpty(workingDirectory))
                    startInfo.WorkingDirectory = workingDirectory;

                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception e)
            {
                Debug.LogError($"启动新的游戏进程失败，请手动重新打开游戏：{e.Message}");
            }

            Application.Quit();
#else
            Debug.LogError("当前平台不支持自动重启，请手动重新打开游戏。");
#endif
        }
    }
}
