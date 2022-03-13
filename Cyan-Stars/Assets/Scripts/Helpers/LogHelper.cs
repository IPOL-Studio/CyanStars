using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LogLevelType
{
    Log, Warning, Error
}

public static class LogHelper
{
    public static NoteLogger NoteLogger { get; } = new NoteLogger();
    
    public static void Log(string message, LogLevelType logLevel)
    {
        switch (logLevel)
        {
            case LogLevelType.Log:
                Debug.Log(message);
                break;
            case LogLevelType.Warning:
                Debug.LogWarning(message);
                break;
            case LogLevelType.Error:
                Debug.LogError(message);
                break;
            default:
                Debug.LogError($"undefined Log, Type: {logLevel}, message: {message}");
                break;
        }
    }
}

public class NoteLogger
{
    public LogLevelType LogLevel { get; set; } = LogLevelType.Error;

    public void Log<T>(T args) where T : struct, INoteJudgeLogArgs
    {
        LogHelper.Log(args.GetJudgeInfo(), LogLevel);
    }
}

public interface INoteJudgeLogArgs
{
    public string GetJudgeInfo();
}

public struct DefaultNoteJudgeLogArgs : INoteJudgeLogArgs
{
    private NoteData noteData;
    private EvaluateType evaluate;

    public DefaultNoteJudgeLogArgs(NoteData data, EvaluateType evaluate)
    {
        this.noteData = data;
        this.evaluate = evaluate;
    }
        
    public string GetJudgeInfo()
    {
        var holdEndTime = noteData.Type == NoteType.Hold ? $",Hold音符结束时间{noteData.HoldEndTime}" : string.Empty;
        return $"{noteData.Type}音符{evaluate}，音符数据：位置{noteData.Pos}，宽度{noteData.Width},开始时间{noteData.StartTime}{holdEndTime}";
    }
}

public struct ClickNoteJudgeLogArgs : INoteJudgeLogArgs
{
    private NoteData noteData;
    private EvaluateType evaluate;
    private float holdTime;

    public ClickNoteJudgeLogArgs(NoteData data, EvaluateType evaluate, float holdTime = 0)
    {
        this.noteData = data;
        this.evaluate = evaluate;
        this.holdTime = holdTime;
    }
    
    public string GetJudgeInfo()
    {
        return evaluate == EvaluateType.Miss 
            ? $"Click音符miss: 音符数据：位置{noteData.Pos}，宽度{noteData.Width},开始时间{noteData.StartTime}" 
            : $"Click音符命中，按住时间：{holdTime}: 音符数据：位置{noteData.Pos}，宽度{noteData.Width},开始时间{noteData.StartTime}";
    }
}

public struct ClickNoteHeadJudgeLogArgs : INoteJudgeLogArgs
{
    private NoteData noteData;
    private EvaluateType evaluate;
    private float logicTime;

    public ClickNoteHeadJudgeLogArgs(NoteData data, EvaluateType evaluate, float logicTime)
    {
        this.noteData = data;
        this.evaluate = evaluate;
        this.logicTime = logicTime;
    }
    
    public string GetJudgeInfo()
    {
        return evaluate == EvaluateType.Bad || evaluate == EvaluateType.Miss
            ? $"Click头判失败:时间：{logicTime}，音符数据：位置{noteData.Pos}，宽度{noteData.Width},开始时间{noteData.StartTime}" 
            : $"Click头判命中，{logicTime}: 音符数据：位置{noteData.Pos}，宽度{noteData.Width},开始时间{noteData.StartTime}";
    }
}

public struct HoldNoteJudgeLogArgs : INoteJudgeLogArgs
{
    private NoteData noteData;
    private EvaluateType evaluate;
    private float holdLength;

    public HoldNoteJudgeLogArgs(NoteData data, EvaluateType evaluate, float holdLength)
    {
        this.noteData = data;
        this.evaluate = evaluate;
        this.holdLength = holdLength;
    }
    
    public string GetJudgeInfo()
    {
        return evaluate == EvaluateType.Miss
            ? $"Hold音符miss: 音符数据：位置{noteData.Pos}，宽度{noteData.Width},开始时间{noteData.StartTime},Hold音符结束时间{noteData.HoldEndTime}"
            : $"Hold音符命中，百分比:{holdLength},评价:{evaluate},音符数据：位置{noteData.Pos}，宽度{noteData.Width},开始时间{noteData.StartTime},Hold音符结束时间{noteData.HoldEndTime}";
    }
}

public struct HoldNoteHeadJudgeLogArgs : INoteJudgeLogArgs
{
    private NoteData noteData;
    private EvaluateType evaluate;
    private float logicTime;

    public HoldNoteHeadJudgeLogArgs(NoteData data, EvaluateType evaluate, float logicTime)
    {
        this.noteData = data;
        this.evaluate = evaluate;
        this.logicTime = logicTime;
    }
    
    public string GetJudgeInfo()
    {
        return $"Hold头判{(evaluate == EvaluateType.Bad || evaluate == EvaluateType.Miss ? "失败" : "成功")}，时间：{logicTime}，音符数据：位置{noteData.Pos}，宽度{noteData.Width},开始时间{noteData.StartTime},Hold音符结束时间{noteData.HoldEndTime}";
    }
}
