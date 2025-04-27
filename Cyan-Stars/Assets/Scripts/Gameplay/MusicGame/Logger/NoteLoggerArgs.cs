//
//
// namespace CyanStars.Gameplay.MusicGame
// {
//     public interface INoteJudgedInfo
//     {
//         public string GetJudgeMessage();
//     }
//
//     public struct DefaultNoteJudgedInfo : INoteJudgedInfo
//     {
//         private NoteData noteData;
//         private EvaluateType evaluate;
//
//         public DefaultNoteJudgedInfo(NoteData data, EvaluateType evaluate)
//         {
//             this.noteData = data;
//             this.evaluate = evaluate;
//         }
//
//         public string GetJudgeMessage()
//         {
//             return $"{noteData.Type}音符{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}";
//         }
//     }
//
//     public struct ClickNoteJudgedInfo : INoteJudgedInfo
//     {
//         private NoteData noteData;
//         private EvaluateType evaluate;
//         private float holdTime;
//
//         public ClickNoteJudgedInfo(NoteData data, EvaluateType evaluate, float holdTime)
//         {
//             this.noteData = data;
//             this.evaluate = evaluate;
//             this.holdTime = holdTime;
//         }
//
//         public string GetJudgeMessage()
//         {
//             return $"Click尾判{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}, 按住时间{holdTime}";
//         }
//     }
//
//     public struct ClickNoteHeadJudgedInfo : INoteJudgedInfo
//     {
//         private NoteData noteData;
//         private EvaluateType evaluate;
//
//         public ClickNoteHeadJudgedInfo(NoteData data, EvaluateType evaluate)
//         {
//             this.noteData = data;
//             this.evaluate = evaluate;
//         }
//
//         public string GetJudgeMessage()
//         {
//             return $"Click头判{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}";
//         }
//     }
//
//     public struct HoldNoteJudgedInfo : INoteJudgedInfo
//     {
//         private NoteData noteData;
//         private EvaluateType evaluate;
//         private float holdTime;
//         private float holdRatio;
//
//         public HoldNoteJudgedInfo(NoteData data, EvaluateType evaluate, float holdTime, float holdRatio)
//         {
//             this.noteData = data;
//             this.evaluate = evaluate;
//             this.holdTime = holdTime * 1000;
//             this.holdRatio = holdRatio;
//         }
//
//         public string GetJudgeMessage()
//         {
//             return
//                 $"Hold尾判{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}, 结束时间{noteData.HoldEndTime}, 按住时间{holdTime}, 按住比例{holdRatio}";
//         }
//     }
//
//     public struct HoldNoteHeadJudgedInfo : INoteJudgedInfo
//     {
//         private NoteData noteData;
//         private EvaluateType evaluate;
//
//         public HoldNoteHeadJudgedInfo(NoteData data, EvaluateType evaluate)
//         {
//             this.noteData = data;
//             this.evaluate = evaluate;
//         }
//
//         public string GetJudgeMessage()
//         {
//             return $"Hold头判{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}, 结束时间{noteData.HoldEndTime}";
//         }
//     }
// }
