using UnityEngine;

namespace CyanStars.Gameplay
{
    partial class MusicGameSceneConfigure
    {
        [System.Serializable]
        public class GameMainTrackBounds
        {
            [SerializeField] private Transform leftTrans; //左端点

            [SerializeField] private Transform rightTrans; //右端点

            /// <summary>
            /// 获取左端点的坐标
            /// </summary>
            public Vector3 LeftPos => leftTrans.position;

            /// <summary>
            /// 获取右端点的坐标
            /// </summary>
            public Vector3 RightPos => rightTrans.position;

            /// <summary>
            /// 场景宽度
            /// </summary>
            public float Length => Mathf.Abs(RightPos.x - LeftPos.x);

            /// <summary>
            /// 获取指定比例的X坐标
            /// </summary>
            public float GetPosWithRatio(float ratio) => LeftPos.x + ratio * Length;
        }

        [System.Serializable]
        public class GameBreakTracks
        {
            [SerializeField] private Vector2 leftPos;
            [SerializeField] private Vector2 rightPos;
            [SerializeField] private Vector3 leftRot;
            [SerializeField] private Vector3 rightRot;

            public Vector2 LeftPos => leftPos;
            public Vector2 RightPos => rightPos;
            public Vector3 LeftRot => leftRot;
            public Vector3 RightRot => rightRot;

            /// <summary>
            /// 0 -> left
            /// <para>1 -> right</para>
            /// </summary>
            /// <exception cref="System.ArgumentOutOfRangeException">index is not 0 or 1</exception>
            public Vector3 GetPosition(int index) => index switch {
                0 => LeftPos,
                1 => RightPos,
                _ => throw new System.ArgumentOutOfRangeException(nameof(index))
            };

            /// <summary>
            /// 0 -> left
            /// <para>1 -> right</para>
            /// </summary>
            /// <exception cref="System.ArgumentOutOfRangeException">index is not 0 or 1</exception>
            public Vector3 GetRotation(int index) => index switch {
                0 => LeftRot,
                1 => RightRot,
                _ => throw new System.ArgumentOutOfRangeException(nameof(index))
            };
        }
    }


    public partial class MusicGameSceneConfigure : MonoBehaviour
    {
        [SerializeField] private GameMainTrackBounds mainTrackBounds;
        [SerializeField] private GameBreakTracks breakTracks;

        public GameMainTrackBounds MainTrackBounds => mainTrackBounds;
        public GameBreakTracks BreakTracks => breakTracks;
    }
}
