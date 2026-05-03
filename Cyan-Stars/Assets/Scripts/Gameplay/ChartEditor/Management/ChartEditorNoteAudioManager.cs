using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.Management
{
    // TODO: 这个类现在的实现非常 hack，只是为了让制谱器有打击音可用
    // 没有优化，也无扩展
    public class ChartEditorNoteAudioManager : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource = null!;


        private ChartEditorModel model = null!;
        private int prevTimeMs = 0;
        private int skipNoteCount = 0;

        private const string FallbackAudioAssetName = "Assets/BundleRes/Audio/PromptTone/ns_ka.ogg";

        // 预加载内置提示音，但在实际播放时不会使用这个 handler
        // 只是为了避免被资源框架卸载
        private AssetHandler<AudioClip> fallbackAudioAssetHandler;

        private float audioVolume = 1f;

        public float AudioVolume
        {
            get => audioVolume;
            set => audioVolume = Math.Clamp(value, 0f, 1f);
        }

        public void Init(ChartEditorModel chartEditorModel)
        {
            fallbackAudioAssetHandler = GameRoot.Asset.LoadAssetAsync<AudioClip>(FallbackAudioAssetName).AddTo(this);

            chartEditorModel.IsTimelinePlaying
                .Subscribe(toPlay =>
                {
                    if (!toPlay)
                        return;

                    prevTimeMs = model.CurrentTimelineTimeMs;
                    skipNoteCount = GetSkippedNoteCount(prevTimeMs);
                })
                .AddTo(this);

            model = chartEditorModel;
        }

        private int GetSkippedNoteCount(int toTimeMs)
        {
            var notes = model.ChartData.CurrentValue.Notes;
            var bpmGroups = model.ChartPackData.CurrentValue.BpmGroup;
            int count = 0;
            foreach (var note in notes)
            {
                var judgeTime = BpmGroupHelper.CalculateTime(bpmGroups, note.JudgeBeat);
                if (judgeTime > toTimeMs)
                    break;

                count++;
            }

            return count;
        }

        public void LateUpdate()
        {
            if (model is null || !model.IsTimelinePlaying.CurrentValue)
                return;

            if (model.CurrentTimelineTimeMs == prevTimeMs)
                return;

            if (skipNoteCount >= model.ChartData.CurrentValue.Notes.Count)
                return;

            // 作为临时的 hack 实现，这里可以选择只收集需要播放的数量
            // 但还是直接把所有 note 收集起来
            // 之后可以看看能不能把 notes 的运行时存储改为时间轮
            foreach (var note in CollectHitNotes(model.CurrentTimelineTimeMs))
            {
                // TODO: 后续考虑用 audioSource 对象池 + PlayScheduled 提供更高精度的音效
                // GameRoot.Audio.Play2DSound(FallbackAudioAssetName, AudioVolume);
                audioSource.PlayOneShot(fallbackAudioAssetHandler.Asset, AudioVolume);
            }
        }

        public IEnumerable<BaseChartNoteData> CollectHitNotes(int toTimeMs)
        {
            var notes = model.ChartData.CurrentValue.Notes;
            var bpmGroups = model.ChartPackData.CurrentValue.BpmGroup;
            int index = skipNoteCount;
            while (index < notes.Count)
            {
                var note = notes[index];
                var judgeTime = BpmGroupHelper.CalculateTime(bpmGroups, note.JudgeBeat);

                if (note.JudgeAble)
                {
                    if (judgeTime > toTimeMs)
                        break;

                    yield return note;
                }

                index++;
            }

            skipNoteCount = index;
        }
    }
}
