using System;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using DG.Tweening;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    public class AudioController : MonoBehaviour
    {
        private DialogueModule dataModule;

        [SerializeField]
        private AudioSource mainMusicSource;

        [SerializeField]
        private AudioSource crossMusicSource;

        [SerializeField]
        private AudioSource soundSource;


        private Tween musicFadeTween;
        private AudioSource curMainMusicSource;

        private void Awake()
        {
            dataModule = GameRoot.GetDataModule<DialogueModule>();

            GameRoot.Event.AddListener(EventConst.PlaySoundEvent, OnPlaySound);
            GameRoot.Event.AddListener(PlayMusicEventArgs.EventName, OnPlayMusic);
        }

        private void Start()
        {
            curMainMusicSource = mainMusicSource;
        }

        private void OnDestroy()
        {
            GameRoot.Event.RemoveListener(EventConst.PlaySoundEvent, OnPlaySound);
            GameRoot.Event.RemoveListener(PlayMusicEventArgs.EventName, OnPlayMusic);
        }

        private void OnPlaySound(object sender, EventArgs e)
        {
            var path = (e as SingleEventArgs<string>)!.Value;

            if (dataModule.TryGetSoundAudioClip(path, out var clip))
            {
                soundSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogError($"没有找到音效 ${path}");
            }
        }

        private void OnPlayMusic(object sender, EventArgs e)
        {
            var eventArgs = e as PlayMusicEventArgs;

            bool hasNextMusic = dataModule.TryGetMusicAudioClip(eventArgs!.FilePath, out var clip);

            if (!hasNextMusic) // next music not found, fade out
            {
                MusicFadeOut(eventArgs.FadeOutTime);
            }
            else
            {
                if (curMainMusicSource.clip == null) // next music founded, but current no music playing, fade in
                {
                    MusicFadeIn(eventArgs.FadeInTime, clip);
                }
                else if (eventArgs.IsCrossFading && eventArgs.FadeInTime > 0)
                {
                    MusicCrossFading(eventArgs.FadeInTime, clip);
                }
                else
                {
                    MusicFadeOut(eventArgs.FadeOutTime, () =>
                    {
                        MusicFadeIn(eventArgs.FadeInTime, clip);
                    });
                }
            }
        }

        private void MusicFadeOut(float duration, Action onComplete = null)
        {
            if (curMainMusicSource.clip != null && duration > 0)
            {
                musicFadeTween = DoMusicFadeOut(duration, curMainMusicSource, () =>
                {
                    musicFadeTween = null;
                    onComplete?.Invoke();
                });
                musicFadeTween.Play();
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private void MusicFadeIn(float duration, AudioClip clip)
        {
            curMainMusicSource.Stop();
            curMainMusicSource.clip = clip;

            if (duration > 0)
            {
                musicFadeTween = DoMusicFadeIn(duration, curMainMusicSource, () =>
                {
                    musicFadeTween = null;
                });
                musicFadeTween.Play();
            }
        }

        private void MusicCrossFading(float duration, AudioClip clip)
        {
            var seq = DOTween.Sequence();

            var fadeOutSource = curMainMusicSource;
            var fadeOut = DoMusicFadeOut(duration, fadeOutSource, null);

            SwitchMainMusicSource();

            var fadeInSource = curMainMusicSource;
            fadeInSource.Stop();
            fadeInSource.clip = clip;
            var fadeIn = DoMusicFadeIn(duration, curMainMusicSource, null);

            musicFadeTween = seq
                .Insert(0, fadeOut)
                .Insert(0, fadeIn)
                .OnComplete(() =>
                {
                    musicFadeTween = null;
                });

            musicFadeTween.Play();
        }

        private Tweener DoMusicFadeIn(float duration, AudioSource audioSource, Action onComplete)
        {
            audioSource.volume = 0;
            audioSource.Play();
            //TODO: Music volume setting
            return audioSource.DOFade(1, duration).OnComplete(() => onComplete?.Invoke());
        }

        private Tweener DoMusicFadeOut(float duration, AudioSource audioSource, Action onComplete)
        {
            var volume = audioSource.volume;
            return audioSource.DOFade(0, duration).OnComplete(() =>
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.volume = volume;
                onComplete?.Invoke();
            });
        }

        private void SwitchMainMusicSource()
        {
            curMainMusicSource = curMainMusicSource == mainMusicSource ? crossMusicSource : mainMusicSource;
        }
    }
}
