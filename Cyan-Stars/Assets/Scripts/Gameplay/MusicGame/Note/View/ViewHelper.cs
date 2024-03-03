using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CyanStars.Framework;

using UnityEngine;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 视图层辅助类
    /// </summary>
    public static class ViewHelper
    {
        /// <summary>
        /// 创建视图层物体的时间距离
        /// </summary>
        public const float ViewObjectCreateDistance = 200;

        /// <summary>
        /// 视图层物体根节点
        /// </summary>
        public static Transform ViewRoot { get; set; }

        /// <summary>
        /// 特效根节点
        /// </summary>
        public static Transform EffectRoot { get; set; }

        /// <summary>
        /// 创建视图层物体
        /// </summary>
        public static async Task<IView> CreateViewObject(NoteData data, BaseNote note)
        {
            MusicGameModule dataModule = GameRoot.GetDataModule<MusicGameModule>();
            MusicGameSceneConfigure sceneConfigure = dataModule.SceneConfigure;

            GameObject go = null;
            string prefabName = dataModule.NotePrefabNameDict[data.Type];

            go = await GameRoot.GameObjectPool.GetGameObjectAsync(prefabName,ViewRoot);
            Transform trans = go.transform;

            //这里因为用了异步await，所以需要使用note在物体创建成功后这一刻的视图层时间作为viewCreateTime，否则位置会对不上
            trans.position = GetViewObjectPos(data,note.ViewDistance, sceneConfigure);
            trans.localScale = GetViewObjectScale(data, sceneConfigure.MainTrackBounds.Length);
            trans.localEulerAngles = GetViewObjectRotation(data, sceneConfigure);

            ViewObject view = go.GetComponent<ViewObject>();

            if (data.Type == NoteType.Hold)
            {
                float length = (data.HoldViewEndTime - data.ViewJudgeTime) / 1000f;
                (view as HoldViewObject).SetLength(length);
            }

            return view;
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层位置
        /// </summary>
        private static Vector3 GetViewObjectPos(NoteData data,float viewDistance, MusicGameSceneConfigure sceneConfigure)
        {
            float z = viewDistance;

            if (data.Type == NoteType.Break)
            {
                Vector2 trackPos = sceneConfigure.BreakTracks.GetPosition(GetBreakTrackIndex(data.Pos));
                return new Vector3(trackPos.x, trackPos.y, z);
            }
            else
            {
                var bounds = sceneConfigure.MainTrackBounds;
                return new Vector3(bounds.GetPosWithRatio(data.Pos), bounds.LeftPos.y, z);
            }
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层缩放
        /// </summary>
        private static Vector3 GetViewObjectScale(NoteData data, float mainTrackLength)
        {
            Vector3 scale = Vector3.one;

            if (data.Type != NoteType.Break)
            {
                //非Break音符需要缩放宽度
                scale.x = NoteData.NoteWidth * mainTrackLength;
                scale.y = 2;
            }
            else
            {
                scale.x = 1;
                scale.z = 1;
            }

            return scale;
        }

        /// <summary>
        /// 根据音符数据获取视图层物体旋转
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetViewObjectRotation(NoteData data, MusicGameSceneConfigure sceneConfigure)
        {
            return data.Type == NoteType.Break
                ? sceneConfigure.BreakTracks.GetRotation(GetBreakTrackIndex(data.Pos))
                : Vector3.zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBreakTrackIndex(float pos)
        {
            return Mathf.Abs(pos - (-1)) < float.Epsilon ? 0 : 1;
        }
    }
}
