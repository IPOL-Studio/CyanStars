using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 序列帧播放器
    /// </summary>
    /// <remarks>
    /// 从对象池中取出特效时（物体被激活）会自动从头播放，并按 <see cref="frameRate"/> 逐帧切换 Sprite
    /// </remarks>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSequencePlayer : MonoBehaviour
    {
        /// <summary>
        /// 序列帧图片，按播放顺序排列
        /// </summary>
        [SerializeField]
        private Sprite[] frames;

        /// <summary>
        /// 播放帧率（帧/秒）
        /// </summary>
        [SerializeField]
        private float frameRate = 60f;

        /// <summary>
        /// 是否循环播放（否则播放完最后一帧后停止）
        /// </summary>
        [SerializeField]
        private bool loop;

        /// <summary>
        /// 是否在被激活时自动从头播放
        /// </summary>
        [SerializeField]
        private bool playOnEnable = true;

        /// <summary>
        /// 播放结束后是否隐藏渲染器（最后一帧通常为空帧）
        /// </summary>
        [SerializeField]
        private bool hideOnFinish = true;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        /// <summary>
        /// 已播放的时间（s）
        /// </summary>
        private float playTime;

        /// <summary>
        /// 当前显示的帧索引
        /// </summary>
        private int frameIndex;

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying { get; private set; }

        private void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// 从头开始播放
        /// </summary>
        public void Play()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null || frames == null || frames.Length == 0)
            {
                IsPlaying = false;
                return;
            }

            playTime = 0;
            frameIndex = 0;
            IsPlaying = true;

            spriteRenderer.enabled = true;
            spriteRenderer.sprite = frames[0];
        }

        /// <summary>
        /// 停止播放，保持当前显示的帧
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
        }

        private void Update()
        {
            if (!IsPlaying)
                return;

            // 根据播放时长计算应显示的帧，避免逐帧累加造成的误差
            playTime += Time.deltaTime;
            int index = Mathf.FloorToInt(playTime * Mathf.Max(1f, frameRate));

            if (index >= frames.Length)
            {
                if (!loop)
                {
                    Finish();
                    return;
                }

                index %= frames.Length;
            }

            if (index == frameIndex)
                return;

            frameIndex = index;
            spriteRenderer.sprite = frames[index];
        }

        /// <summary>
        /// 播放结束
        /// </summary>
        private void Finish()
        {
            IsPlaying = false;

            frameIndex = frames.Length - 1;
            spriteRenderer.sprite = frames[frameIndex];

            if (hideOnFinish)
                spriteRenderer.enabled = false;
        }
    }
}
