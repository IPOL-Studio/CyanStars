using System;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using DG.Tweening;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    public class AudioManager : MonoBehaviour, IService
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

        private void Start()
        {
            dataModule = GameRoot.GetDataModule<DialogueModule>();
            curMainMusicSource = mainMusicSource;
        }

        public void OnRegister()
        {
        }

        public void OnUnregister()
        {
        }

        public void PlaySound(string filePath)
        {
            if (dataModule.TryGetSoundAudioClip(filePath, out var clip))
            {
                soundSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogError($"没有找到音效 ${filePath}");
            }
        }

        public void PlayMusic(PlayMusicArgs args)
        {
            bool hasNextMusic = dataModule.TryGetMusicAudioClip(args.FilePath, out var clip);

            if (!hasNextMusic) // next music not found, fade out
            {
                MusicFadeOut(args.FadeOutTime);
            }
            else
            {
                if (curMainMusicSource.clip == null) // next music founded, but current no music playing, fade in
                {
                    MusicFadeIn(args.FadeInTime, clip);
                }
                else if (args.IsCrossFading && args.FadeInTime > 0)
                {
                    MusicCrossFading(args.FadeInTime, clip);
                }
                else
                {
                    MusicFadeOut(args.FadeOutTime, () =>
                    {
                        MusicFadeIn(args.FadeInTime, clip);
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
