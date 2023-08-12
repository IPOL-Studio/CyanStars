using System.Collections.Generic;
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
            GameObject go = null;
            string prefabName = dataModule.NotePrefabNameDict[data.Type];

            go = await GameRoot.GameObjectPool.GetGameObjectAsync(prefabName, ViewRoot);

            //这里因为用了异步await，所以需要使用note在物体创建成功后这一刻的视图层时间作为viewCreateTime，否则位置会对不上
            go.transform.position = GetViewObjectPos(data, note.ViewDistance);
            go.transform.localScale = GetViewObjectScale(data);
            go.transform.localEulerAngles = GetViewObjectRotation(data);

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
        private static Vector3 GetViewObjectPos(NoteData data, float viewDistance)
        {
            Vector3 pos = default;

            pos.z = viewDistance;

            pos.y = Endpoint.Instance.LeftTrans.position.y;
            if (data.Type == NoteType.Break)
            {
                if (Mathf.Abs(data.Pos - (-1)) < float.Epsilon)
                {
                    //左侧break
                    pos.x = -19;
                }
                else
                {
                    //右侧break
                    pos.x = 19;
                }

                pos.y = 2.8f;
            }
            else
            {
                pos.x = Endpoint.Instance.GetPosWithRatio(data.Pos);
            }

            return pos;
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层缩放
        /// </summary>
        private static Vector3 GetViewObjectScale(NoteData data)
        {
            Vector3 scale = Vector3.one;

            if (data.Type == NoteType.Break)
            {
                //Break音符不需要缩放xyz轴
                scale.x = 1;
                scale.y = 1;
                scale.z = 1;
            }
            else if (data.Type == NoteType.Drag)
            {
                //Drag音符不需要缩放z轴
                scale.x = NoteData.NoteWidth * Endpoint.Instance.Length;
                scale.y = 2;
                scale.z = 1;
            }
            else
            {
                //其他音符需要缩放xyz轴
                scale.x = NoteData.NoteWidth * Endpoint.Instance.Length;
                scale.y = 2;
                scale.z = 2.5f;
            }

            return scale;
        }

        /// <summary>
        /// 根据音符数据获取视图层物体旋转
        /// </summary>
        private static Vector3 GetViewObjectRotation(NoteData data)
        {
            Vector3 rotation = Vector3.zero;
            if (data.Type == NoteType.Break)
            {
                if (Mathf.Abs(data.Pos - (-1)) < float.Epsilon)
                {
                    //左侧break
                    rotation.z = -28;
                }
                else
                {
                    //右侧break
                    rotation.z = 28;
                }
            }

            return rotation;
        }
    }
}
