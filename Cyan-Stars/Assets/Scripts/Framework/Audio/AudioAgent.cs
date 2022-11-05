using System;
using CatAsset.Runtime;
using UnityEngine;

namespace CyanStars.Framework.Audio
{
    /// <summary>
    /// 音频代理
    /// </summary>
    public class AudioAgent : MonoBehaviour
    {
        private enum State
        {
            None,
            Playing,
            Paused,
            Stop,
        }

        [SerializeField]
        internal AudioSource Source;
        
        [SerializeField]
        private State state;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// 是否循环
        /// </summary>
        public bool Loop
        {
            get => Source.loop;
            set => Source.loop = value;
        }

        /// <summary>
        /// 是否静音
        /// </summary>
        public bool Mute
        {
            get => Source.mute;
            set => Source.mute = value;
        }
        
        /// <summary>
        /// 音量
        /// </summary>
        public float Volume
        {
            get => Source.volume;
            set => Source.volume = value;
        }

        internal void OnUpdate()
        {
            if (!IsValid)
            {
                return;
            }

            if (Loop)
            {
                return;
            }

            if (state != State.Playing)
            {
                return;
            }

            if (!Source.isPlaying)
            {
                //当前AudioAgent处于Playing状态 但是AudioSource已经播放结束了 就调用StopAudio
                StopAudio();
            }
        }

        internal void PlayAudio(float volume,bool loop)
        {
            Volume = volume;
            Loop = loop;
            IsValid = true;
            state = State.Playing;
            
            Source.Play();
        }

        /// <summary>
        /// 暂停音频
        /// </summary>
        public void PauseAudio()
        {
            if (!IsValid)
            {
                return;
            }
            
            if (state == State.Paused)
            {
                return;
            }

            state = State.Paused;
            Source.Pause();
        }

        /// <summary>
        /// 恢复音频
        /// </summary>
        public void ResumeAudio()
        {
            if (!IsValid)
            {
                return;
            }
            
            if (state != State.Paused)
            {
                return;
            }

            state = State.Playing;
            Source.UnPause();
        }

        /// <summary>
        /// 结束音频
        /// </summary>
        public void StopAudio()
        {
            IsValid = false;
            state = State.Stop;
            Source.Stop();
        }

        internal void Reset()
        {
            gameObject.transform.position = default;
            
            Source.clip = default;
            Source.time = default;
            Volume = default;
            Loop = default;
            Mute = default;
            Source.spatialBlend = default;
          
            
            state = default;
            IsValid = default;
        }
    }
}