using UnityEngine;
using CyanStars.Gameplay.PromptTone;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 音符数据
    /// </summary>
    [System.Serializable]
    public class NoteData
    {
        public const float MaxPos = 1;
        public const float MiddlePos = MaxPos / 2;

        /// <summary>
        /// 音符类型
        /// </summary>
        public NoteType Type;

        /// <summary>
        /// 位置坐标
        /// 处于中间的位置范围在[0-1]，Break音符则左-1右2
        /// </summary>
        [Range(-1, 2)] public float Pos;

        /// <summary>
        /// 音符宽度
        /// 该值不变，恒为0.2
        /// </summary>
        //[Range(0,1)]
        [Header("不要改变这个值，这个值不变，恒为0.2")] public float Width = 0.2f;

        /// <summary>
        /// 判定开始时间（毫秒）
        /// </summary>
        public int StartTime;

        /// <summary>
        /// Hold音符的判定结束时间（毫秒）
        /// </summary>
        public int HoldEndTime;

        /// <summary>
        /// 提示音
        /// </summary>
        public PromptToneType PromptToneType = PromptToneType.NsKa;

        public float GetFullScore()
        {
            return Type switch
            {
                NoteType.Tap => 1,
                NoteType.Hold => 2,
                NoteType.Break => 2,
                NoteType.Drag => 0.25f,
                NoteType.Click => 2f,
                _ => throw new System.NotFiniteNumberException()
            };
        }


        public override string ToString()
        {
            return $"音符数据：类型{Type}，位置{Pos}，宽度{Width},开始时间{StartTime},Hold音符结束时间{HoldEndTime}";
        }
    }
}
