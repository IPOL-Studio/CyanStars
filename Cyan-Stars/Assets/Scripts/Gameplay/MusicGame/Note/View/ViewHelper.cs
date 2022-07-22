using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.GameObjectPool;

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
            string prefabName = data.Type switch
            {
                NoteType.Tap => dataModule.TapPrefabName,
                NoteType.Hold => dataModule.HoldPrefabName,
                NoteType.Drag => dataModule.DragPrefabName,
                NoteType.Click => dataModule.ClickPrefabName,
                NoteType.Break => dataModule.BreakPrefabName,
                _ => null
            };

            go = await GameRoot.GameObjectPool.AwaitGetGameObject(prefabName,ViewRoot);
            //go.transform.SetParent(ViewRoot);

            //这里因为用了异步await，所以需要使用note在物体创建成功后这一刻的视图层时间作为viewCreateTime，否则位置会对不上
            go.transform.position = GetViewObjectPos(data,note.ViewDistance);
            go.transform.localScale = GetViewObjectScale(data);
            go.transform.localEulerAngles = GetViewObjectRotation(data);

            var view = go.GetComponent<ViewObject>();
            view.PrefabName = prefabName;

            if (data.Type == NoteType.Hold)
            {
                //(view as HoldViewObject).SetMesh(1f, endTime - startTime);
                float length = (data.HoldViewEndTime - data.ViewJudgeTime) / 1000f;
                (view as HoldViewObject).SetLength(length);
            }

            return view;
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层位置
        /// </summary>
        private static Vector3 GetViewObjectPos(NoteData data,float viewDistance)
        {
            Vector3 pos = default;

            pos.z = viewDistance;

            pos.y = Endpoint.Instance.LeftTrans.position.y;
            if (data.Type == NoteType.Break)
            {
                if (Mathf.Abs(data.Pos - (-1)) < float.Epsilon)
                {
                    //左侧break
                    pos.x = -15;
                }
                else
                {
                    //右侧break
                    pos.x = 15;
                }

                pos.y = 4;
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

            if (data.Type != NoteType.Break)
            {
                //非Break音符需要缩放宽度
                scale.x = NoteData.NoteWidth * Endpoint.Instance.Length;
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
