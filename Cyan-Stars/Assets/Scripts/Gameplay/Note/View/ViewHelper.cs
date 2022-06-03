using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.GameObjectPool;
using CyanStars.Gameplay.Data;

using UnityEngine;


namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 视图层辅助类
    /// </summary>
    public static class ViewHelper
    {
        private static Dictionary<NoteData, float> viewStartTimeDict = new Dictionary<NoteData, float>();
        private static Dictionary<NoteData, float> viewHoldEndTimeDict = new Dictionary<NoteData, float>();


        /// <summary>
        /// 视图层物体创建倒计时时间（是受速率影响的时间）
        /// </summary>
        public const float ViewObjectCreateTime = 200;

        /// <summary>
        /// 视图层物体根节点
        /// </summary>
        public static Transform ViewRoot { get; set; }

        /// <summary>
        /// 特效根节点
        /// </summary>
        public static Transform EffectRoot { get; set; }

        /// <summary>
        /// 计算受速率影响的视图层音符开始时间和结束时间，用于视图层物体计算位置和长度
        /// </summary>
        public static void CalViewTime(float time,NoteTrackData data)
        {
            viewStartTimeDict.Clear();
            viewHoldEndTimeDict.Clear();

            float timelineSpeedRate = data.BaseSpeed * data.SpeedRate;

            foreach (NoteLayerData layerData in data.LayerDatas)
            {
                //从第一个TimeAxis到前一个TimeAxis 受流速缩放影响后的总时间值（毫秒）
                float scaledTime = 0;

                for (int i = 0; i < layerData.TimeAxisDatas.Count; i++)
                {
                    NoteTimeAxisData curTimeAxisData = layerData.TimeAxisDatas[i];
                    float speedRate = curTimeAxisData.Coefficient * timelineSpeedRate;

                    for (int j = 0; j < curTimeAxisData.NoteDatas.Count; j++)
                    {
                        NoteData noteData = curTimeAxisData.NoteDatas[j];

                        //之前的TimeAxis累计下来的受缩放影响的时间值，再加上当前TimeAxis到当前note这段时间缩放后的时间值
                        //就能得到当前note缩放后的开始时间，因为是毫秒所以要/1000转换为秒
                        float scaledNoteStartTime =
                            scaledTime + ((noteData.JudgeTime - curTimeAxisData.StartTime)) * speedRate;
                        viewStartTimeDict.Add(noteData, scaledNoteStartTime / 1000);
                        //Debug.Log($"逻辑层时间：{noteData.JudgeTime}，视图层时间：{scaledNoteStartTime}");
                        if (noteData.Type == NoteType.Hold)
                        {
                            //hold结束时间同理
                            float scaledHoldNoteEndTime =
                                scaledTime + ((noteData.HoldEndTime - curTimeAxisData.StartTime)) * speedRate;
                            viewHoldEndTimeDict.Add(noteData, scaledHoldNoteEndTime / 1000);
                        }
                    }

                    float curTimeAxisEndTime;
                    if (i < layerData.TimeAxisDatas.Count - 1)
                    {
                        //并非最后一个TimeAxis
                        //将下一个TimeAxis的开始时间作为当前TimeAxis的结束时间
                        curTimeAxisEndTime = layerData.TimeAxisDatas[i + 1].StartTime;
                    }
                    else
                    {
                        //最后一个TimeAxis
                        //将timeline结束时间作为最后一个TimeAxis的结束时间
                        curTimeAxisEndTime = time;
                    }

                    float scaledTimeLength = curTimeAxisEndTime - curTimeAxisData.StartTime;


                    //将此TimeAxis缩放后的时间值 累加到总时间值上
                    scaledTime += scaledTimeLength * speedRate;
                }
            }


        }

        /// <summary>
        /// 获取受速率影响的视图层音符开始时间
        /// </summary>
        public static float GetViewStartTime(NoteData data)
        {
            return viewStartTimeDict[data];
        }

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

            //这里因为用了异步await，所以需要使用note在物体创建成功后这一刻的viewTimer作为viewCreateTime，否则位置会对不上
            go.transform.position = GetViewObjectPos(data, note.ViewTimer);

            go.transform.localScale = GetViewObjectScale(data);
            go.transform.localEulerAngles = GetViewObjectRotation(data);

            var view = go.GetComponent<ViewObject>();
            view.PrefabName = prefabName;

            if (data.Type == NoteType.Hold)
            {
                var startTime = viewStartTimeDict[data];
                var endTime = viewHoldEndTimeDict[data];
                //(view as HoldViewObject).SetMesh(1f, endTime - startTime);
                (view as HoldViewObject).SetLength(endTime - startTime);
            }

            return view;
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层位置
        /// </summary>
        private static Vector3 GetViewObjectPos(NoteData data, float viewCreateTime)
        {
            Vector3 pos = default;

            pos.z = viewCreateTime;

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
