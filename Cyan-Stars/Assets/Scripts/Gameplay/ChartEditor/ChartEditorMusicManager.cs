#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 在播放音乐时自动更新时间，或是在暂停播放时接受时间跳转
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ChartEditorMusicManager : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource = null!;

        private bool isInitialized = false;
        private ChartEditorModel model = null!;
        private double delayTimeMs = 0; // 延迟此时间后播放音乐


        public void Init(ChartEditorModel chartEditorModel)
        {
            model = chartEditorModel;
            model.IsTimelinePlaying
                .Subscribe(isPlaying =>
                    {
                        if (isPlaying)
                            TryPlayMusic(model.CurrentTimelineTime +
                                         model.ChartPackData.CurrentValue.MusicVersions[0].Offset.CurrentValue);
                        else
                            TryPauseMusic();
                    }
                )
                .AddTo(this);

            model.AudioClipHandler
                .Subscribe(handler => audioSource.clip = handler?.Asset)
                .AddTo(this);

            isInitialized = true;
        }

        /// <summary>
        /// 从指定的时间点开始播放音乐
        /// </summary>
        /// <remarks>如果音乐已经在播放，再次调用会跳转，不过一般不会发生</remarks>
        /// <param name="skipMusicTime">开始播放的时间点相对于音乐起始点（ms）。为负数时代表延迟一段时间再开始播放，为正数时代表跳过一段音频时间。</param>
        private void TryPlayMusic(int skipMusicTime)
        {
            if (!audioSource.clip || model.ChartPackData.CurrentValue.MusicVersions.Count == 0)
            {
                Debug.LogError("未指定 Clip 或 MusicVersionData，EditArea 无法播放音频");
                return;
            }

            if (skipMusicTime >= 0)
            {
                // 直接从指定时间开始播放
                audioSource.time = skipMusicTime / 1000f;
                audioSource.Play();
            }
            else
            {
                // 延迟一段时间后从 0 开始播放
                audioSource.Stop();
                delayTimeMs = -skipMusicTime;
            }

            model.IsTimelinePlaying.Value = true;
        }

        /// <summary>
        /// 暂停正在播放的音乐，如果有延迟则取消。
        /// </summary>
        private void TryPauseMusic()
        {
            if (delayTimeMs > 0)
                delayTimeMs = 0;

            if (audioSource.isPlaying)
                audioSource.Pause();

            model.IsTimelinePlaying.Value = false;
        }


        private void Update()
        {
            if (!isInitialized)
                throw new Exception("未完成初始化，请手动调用 Init() 并传入依赖！");

            if (audioSource.time >= audioSource.clip.length)
            {
                // 音乐播完了 // TODO:待测试
                TryPauseMusic();
            }

            // 如果时间轴正在播放，则每帧将倒计时或音频播放进度同步给属性，避免累加误差
            if (!model.IsTimelinePlaying.Value)
                return;

            if (delayTimeMs > 0)
            {
                // 延迟阶段
                delayTimeMs -= Time.unscaledDeltaTime * 1000;

                if (delayTimeMs <= 0)
                {
                    delayTimeMs = 0;
                    audioSource.time = 0;
                    audioSource.Play();
                }

                model.CurrentTimelineTime =
                    model.ChartPackData.CurrentValue.MusicVersions[0].Offset.CurrentValue - (int)delayTimeMs;
            }
            else
            {
                // 正式播放阶段
                model.CurrentTimelineTime =
                    model.ChartPackData.CurrentValue.MusicVersions[0].Offset.CurrentValue + (int)(audioSource.time * 1000);
            }
        }
    }
}
