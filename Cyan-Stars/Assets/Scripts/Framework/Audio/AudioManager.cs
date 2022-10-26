using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.Audio
{
    /// <summary>
    /// 音频管理器
    /// </summary>
    public partial class AudioManager : BaseManager
    {
        /// <summary>
        /// 音频监听器
        /// </summary>
        [SerializeField]
        private AudioListener listener;

        /// <summary>
        /// 音频代理模板
        /// </summary>
        [SerializeField]
        private GameObject audioAgentTemplate;

        /// <summary>
        /// 播放音乐中的音频代理列表
        /// </summary>
        private List<AudioAgent> playingAudioAgents = new List<AudioAgent>();

        public override int Priority { get; }

        public override void OnInit()
        {

        }

        public override void OnUpdate(float deltaTime)
        {
            for (int i = playingAudioAgents.Count - 1; i >= 0; i--)
            {
                AudioAgent agent = playingAudioAgents[i];
                agent.OnUpdate();

                if (!agent.IsValid)
                {
                    playingAudioAgents.RemoveAt(i);
                    ReleaseAgent(agent);
                }
            }
        }

        /// <summary>
        /// 设置音频监听器
        /// </summary>
        public void SetAudioListener(AudioListener listener)
        {
            this.listener = listener;
        }

        /// <summary>
        /// 播放音乐
        /// </summary>
        public void PlayMusicAsync(string musicName,Action<AudioAgent> callback,float volume = 1f,bool loop = true)
        {
            GetAgent(musicName,(agent =>
            {
                agent.PlayAudio(volume,loop);
                callback?.Invoke(agent);
            }));
        }

        /// <summary>
        /// 播放2D音效
        /// </summary>
        public void Play2DSound(string soundName,float volume = 1f)
        {
            GetAgent(soundName,(agent =>
            {
                agent.PlayAudio(volume,false);
            }));
        }

        /// <summary>
        /// 播放3D音效
        /// </summary>
        public void Play3DSound(string soundName,Vector3 position,float volume = 1f)
        {
            GetAgent(soundName,(agent =>
            {
                agent.gameObject.transform.position = position;
                agent.Source.spatialBlend = 1f;
                agent.PlayAudio(volume,false);
            }));
        }

        /// <summary>
        /// 获取音频代理
        /// </summary>
        private void GetAgent(string assetName,Action<AudioAgent> callback,Vector3 position = default)
        {
            GameRoot.Asset.LoadAssetAsync<AudioClip>(assetName, ((asset, result) =>
            {
                if (asset == null)
                {
                    Debug.LogError($"音频资源加载失败:{assetName}");
                    return;
                }

                GameRoot.GameObjectPool.GetGameObjectAsync(audioAgentTemplate,transform,(go =>
                {
                    go.transform.position = position;
                    AudioAgent agent = go.GetComponent<AudioAgent>();
                    agent.Source.clip = asset;
                    playingAudioAgents.Add(agent);
                    callback?.Invoke(agent);
                }));
            }));
        }

        /// <summary>
        /// 释放音频代理
        /// </summary>
        private void ReleaseAgent(AudioAgent agent)
        {
            GameRoot.Asset.UnloadAsset(agent.Source.clip);
            agent.Reset();
            GameRoot.GameObjectPool.ReleaseGameObject(audioAgentTemplate,agent.gameObject);
        }
    }
}


