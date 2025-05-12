// using CyanStars.Gameplay.MusicGame;
// using UnityEngine;
// using UnityEditor;
//
// namespace CyanStars.Editor
// {
//     [CustomEditor(typeof(MapTimelineDataSO))]
//     public class MapTimelineDataSOInspector : UnityEditor.Editor
//     {
//         public override void OnInspectorGUI()
//         {
//             if (GUILayout.Button("计算视图层时间数据"))
//             {
//                 CalViewTime();
//
//                 EditorUtility.SetDirty(target);
//                 AssetDatabase.SaveAssets();
//                 AssetDatabase.Refresh();
//             }
//
//             base.OnInspectorGUI();
//         }
//
//         private void CalViewTime()
//         {
//             MapTimelineDataSO so = (MapTimelineDataSO)target;
//
//             NoteTrackData noteTrackData = so.Data.NoteTrackData;
//
//             foreach (NoteLayerData noteLayerData in noteTrackData.LayerDatas)
//             {
//                 for (int i = 0; i < noteLayerData.TimeAxisDatas.Count; i++)
//                 {
//                     NoteTimeAxisData noteTimeAxisData = noteLayerData.TimeAxisDatas[i];
//
//                     //时轴结束时间
//                     int endTime = so.Data.Length;
//                     if (i != noteLayerData.TimeAxisDatas.Count - 1)
//                     {
//                         //当前结束时间 = 下一时轴开始时间
//                         endTime = noteLayerData.TimeAxisDatas[i + 1].StartTime;
//                     }
//
//                     noteTimeAxisData.EndTime = endTime;
//
//                     //视图层开始时间
//                     int viewStartTime = 0;
//                     if (i != 0)
//                     {
//                         //当前视图层开始时间 = 上一时轴视图层结束时间
//                         viewStartTime = noteLayerData.TimeAxisDatas[i - 1].ViewEndTime;
//                     }
//
//                     noteTimeAxisData.ViewStartTime = viewStartTime;
//
//                     //最终系数 = 基础速度 * 谱面速率 * 时轴函数系数
//                     float finalCoefficient =
//                         noteTrackData.BaseSpeed * noteTrackData.SpeedRate * noteTimeAxisData.Coefficient;
//
//                     //视图层结束时间
//                     int timeLength = noteTimeAxisData.EndTime - noteTimeAxisData.StartTime;
//                     int targetTime = timeLength;
//                     int easingValue = EasingFunction.CalTimeAxisEasingValue(noteTimeAxisData.EasingType,
//                         finalCoefficient,
//                         targetTime, timeLength);
//
//                     //视图层结束时间 = 当前时轴的视图层开始时间 + 时轴的时间长度，应用时轴函数后的值
//                     noteTimeAxisData.ViewEndTime = noteTimeAxisData.ViewStartTime + easingValue;
//
//                     foreach (NoteData noteData in noteTimeAxisData.NoteDatas)
//                     {
//                         //音符视图层时间
//                         targetTime = noteData.JudgeTime - noteTimeAxisData.StartTime;
//                         easingValue = EasingFunction.CalTimeAxisEasingValue(noteTimeAxisData.EasingType,
//                             finalCoefficient,
//                             targetTime, timeLength);
//
//                         //音符视图层时间 = 当前时轴的视图层开始时间 + 当前时轴的开始时间到音符判定时间的时间长度，应用时轴函数后的值
//                         noteData.ViewJudgeTime = noteTimeAxisData.ViewStartTime + easingValue;
//
//                         if (noteData.Type == NoteType.Hold)
//                         {
//                             //Hold音符的视图层结束时间
//                             //计算方式同上
//                             targetTime = noteData.HoldEndTime - noteTimeAxisData.StartTime;
//                             easingValue = EasingFunction.CalTimeAxisEasingValue(noteTimeAxisData.EasingType,
//                                 finalCoefficient,
//                                 targetTime, timeLength);
//                             noteData.HoldViewEndTime = noteTimeAxisData.ViewStartTime + easingValue;
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }
