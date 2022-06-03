using CyanStars.Gameplay.Note;
using CyanStars.Gameplay.PromptTone;
using UnityEngine;
using UnityEngine.Serialization;


namespace CyanStars.Gameplay.Data
{
    /// <summary>
    /// 音符数据
    /// </summary>
    [System.Serializable]
    public class NoteData
    {
        public const float MaxPos = 1;
        public const float MiddlePos = MaxPos / 2;
        public const float NoteWidth = 0.2f;

        /// <summary>
        /// 音符类型
        /// </summary>
        [Header("音符类型")]
        public NoteType Type;

        /// <summary>
        /// 位置坐标
        /// 处于中间的位置范围在[0-1]，Break音符则左-1右2
        /// </summary>
        [Header("位置坐标")]
        [Range(-1, 2)]
        public float Pos;

        /// <summary>
        /// 判定时间（毫秒）
        /// </summary>
        [FormerlySerializedAs("StartTime")]
        [Header("判定时间（毫秒）")]
        public int JudgeTime;

        /// <summary>
        /// Hold音符的判定结束时间（毫秒）
        /// </summary>
        [Header("Hold音符的判定结束时间（毫秒）")]
        public int HoldEndTime;

        /// <summary>
        /// 提示音
        /// </summary>
        [Header("提示音")]
        public PromptToneType PromptToneType = PromptToneType.NsKa;

        /// <summary>
        /// 视图层时间（毫秒）
        /// </summary>
        [Header("视图层时间（毫秒）【不用填】")]
        public int ViewTime;

        /// <summary>
        /// Hold音符的视图层结束时间（毫秒）
        /// </summary>
        [Header("Hold音符的视图层结束时间（毫秒）【不用填】")]
        public int HoldViewEndTime;

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

        public float GetMagnification()
        {
            return Type switch
            {
                NoteType.Tap => 1,
                NoteType.Hold => 1,
                NoteType.Break => 2,
                NoteType.Drag => 0.25f,
                NoteType.Click => 1,
                _ => throw new System.NotFiniteNumberException()
            };
        }


        public override string ToString()
        {
            return $"音符数据：类型{Type}，位置{Pos},判定时间{JudgeTime},Hold音符结束时间{HoldEndTime}";
        }
    }
}
