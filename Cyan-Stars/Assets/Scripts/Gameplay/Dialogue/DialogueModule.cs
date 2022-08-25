using System.Collections.Generic;
using System.Text;
using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueModule : BaseDataModule
    {
        public StringBuilder Content { get; } = new StringBuilder(64);

        /// <summary>
        /// 指示 Content 当前是否被修改过
        /// </summary>
        public bool IsContentDirty { get; set; }

        /// <summary>
        /// 是否为自动模式
        /// </summary>
        public bool IsAutoMode { get; set; } = true;

        /// <summary>
        /// TODO: 临时用于测试的剧本路径
        /// </summary>
        public string StoryDataPath { get; } = "Assets/BundleRes/GalStories/TestStory.json";


        // sound audio file path -> AudioClip
        private readonly Dictionary<string, AudioClip> SoundClipDict = new Dictionary<string, AudioClip>();

        // music audio file path -> AudioClip
        private readonly Dictionary<string, AudioClip> MusicClipDict = new Dictionary<string, AudioClip>();

        public override void OnInit()
        {
            //TODO: init audio clips
        }

        public bool TryGetSoundAudioClip(string filePath, out AudioClip clip)
        {
            return TryGetAudioClip(filePath, SoundClipDict, out clip);
        }

        public bool TryGetMusicAudioClip(string filePath, out AudioClip clip)
        {
            return TryGetAudioClip(filePath, MusicClipDict, out clip);
        }

        private bool TryGetAudioClip(string filePath, Dictionary<string, AudioClip> dict, out AudioClip clip)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            {
                clip = null;
                return false;
            }

            return dict.TryGetValue(filePath, out clip);
        }

        public void Reset()
        {
            Content.Clear();
            IsContentDirty = false;
            SoundClipDict.Clear();
            MusicClipDict.Clear();
        }
    }
}
