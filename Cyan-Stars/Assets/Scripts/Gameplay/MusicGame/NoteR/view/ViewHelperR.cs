using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Gameplay.Chart;
using UnityEngine;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 视图层辅助类
    /// </summary>
    public static class ViewHelperR
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
        public static async Task<IView> CreateViewObject(BaseChartNoteData data, BaseNoteR note)
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
                (view as HoldViewObject).SetLength(
                    Mathf.Abs((note as HoldNoteR).ViewDistance -
                              (note as HoldNoteR).EndViewDistance));
            }

            return view;
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层位置
        /// </summary>
        private static Vector3 GetViewObjectPos(BaseChartNoteData data, float viewDistance)
        {
            Vector3 pos = default;

            pos.z = -viewDistance;

            pos.y = Endpoint.Instance.LeftTrans.position.y;
            switch (data.Type)
            {
                case NoteType.Break:
                {
                    pos.x = ((data as BreakChartNoteData).BreakNotePos == BreakNotePos.Left) ? -19 : 19;
                    break;
                }
                case NoteType.Tap:
                {
                    pos.x = Endpoint.Instance.GetPosWithRatio((data as TapChartNoteData).Pos);
                    break;
                }
                case NoteType.Hold:
                {
                    pos.x = Endpoint.Instance.GetPosWithRatio((data as HoldChartNoteData).Pos);
                    break;
                }
                case NoteType.Drag:
                {
                    pos.x = Endpoint.Instance.GetPosWithRatio((data as DragChartNoteData).Pos);
                    break;
                }
                case NoteType.Click:
                {
                    pos.x = Endpoint.Instance.GetPosWithRatio((data as ClickChartNoteData).Pos);
                    break;
                }
            }

            return pos;
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层缩放
        /// </summary>
        private static Vector3 GetViewObjectScale(BaseChartNoteData _)
        {
            return Vector3.one;
        }

        /// <summary>
        /// 根据音符数据获取视图层物体旋转
        /// </summary>
        private static Vector3 GetViewObjectRotation(BaseChartNoteData data)
        {
            Vector3 rotation = Vector3.zero;
            if (data.Type == NoteType.Break)
            {
                rotation.z = (data as BreakChartNoteData).BreakNotePos == BreakNotePos.Left ? -30 : 30;
            }

            return rotation;
        }
    }
}
