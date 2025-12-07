using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatAsset.Runtime;
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
        private Dictionary<AudioAgent, AssetHandler<AudioClip>> audioAssetHandlerMap = new Dictionary<AudioAgent, AssetHandler<AudioClip>>();

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
            GetAgentAsync(musicName,(agent =>
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
            GetAgentAsync(soundName,(agent =>
            {
                agent.PlayAudio(volume,false);
            }));
        }

        /// <summary>
        /// 播放3D音效
        /// </summary>
        public void Play3DSound(string soundName,Vector3 position,float volume = 1f)
        {
            GetAgentAsync(soundName,(agent =>
            {
                agent.gameObject.transform.position = position;
                agent.Source.spatialBlend = 1f;
                agent.PlayAudio(volume,false);
            }));
        }

        /// <summary>
        /// 获取音频代理
        /// </summary>
        private async void GetAgentAsync(string assetName, Action<AudioAgent> callback, Vector3 position = default)
        {
            var handler = await GameRoot.Asset.LoadAssetAsync<AudioClip>(assetName);
            
            if (!handler.IsSuccess)
            {
                Debug.LogError($"音频资源加载失败:{assetName}");
                return;
            }

            var go = await GameRoot.GameObjectPool.GetGameObjectAsync(audioAgentTemplate,transform);

            go.transform.position = position;
            AudioAgent agent = go.GetComponent<AudioAgent>();
            agent.Source.clip = handler.Asset;
            playingAudioAgents.Add(agent);
            audioAssetHandlerMap.Add(agent, handler);

            callback?.Invoke(agent);
        }

        /// <summary>
        /// 释放音频代理
        /// </summary>
        private void ReleaseAgent(AudioAgent agent)
        {
            if (audioAssetHandlerMap.TryGetValue(agent, out var handler))
            {
                GameRoot.Asset.UnloadAsset(handler);
                audioAssetHandlerMap.Remove(agent);
            }
            agent.Reset();
            GameRoot.GameObjectPool.ReleaseGameObject(audioAgentTemplate,agent.gameObject);
        }
    }
}


