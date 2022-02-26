using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视图层辅助类
/// </summary>
public static class ViewHelper
{
    private static Dictionary<NoteData, float> scaledStartTimeDict = new Dictionary<NoteData, float>();
    private static Dictionary<NoteData, float> scaledHoldEndTimeDict = new Dictionary<NoteData, float>();

      /// <summary>
    /// 计算受速率影响的音符开始时间和结束时间，用于视图层物体计算位置和长度
    /// </summary>
    public static void CalScaledTime(MusicTimelineData data)
    {
        scaledStartTimeDict.Clear();
        scaledHoldEndTimeDict.Clear();
        
        float timelineSpeedRate = data.BaseSpeed * data.SpeedRate;
        
        foreach (LayerData layerData in data.LayerDatas)
        { 
            //从第一个clip到当前clip 受流速缩放影响后的总时间值
            float scaledTime = 0;
            
            for (int i = 0; i < layerData.ClipDatas.Count; i++)
            {
                ClipData curClipData = layerData.ClipDatas[i];
                
                float curClipStartTime = curClipData.StartTime;
                float curClipEndTime;
                if (i < layerData.ClipDatas.Count - 1)
                {
                    //并非最后一个clip
                    //将下一个clip的开始时间作为当前clip的结束时间
                    curClipEndTime = layerData.ClipDatas[i + 1].StartTime;
                }
                else
                {
                    //最后一个clip
                    //将timeline结束时间作为最后一个clip的结束时间
                    curClipEndTime = data.Time;
                }

                float scaledTimeLength = curClipEndTime - curClipStartTime;
                float speedRate = curClipData.SpeedRate * timelineSpeedRate;
                
                //计算受timeline速率和当前clip的速率影响后的时间长度 累加到总时间值上
                scaledTime += scaledTimeLength * speedRate;
                
                for (int j = 0; j < curClipData.NoteDatas.Count; j++)
                {
                    NoteData noteData = curClipData.NoteDatas[j];

                    //将当前note在clip中后面的那段时间从scaledTime里减去，就能得出缩放后的note开始时间
                    float scaledNoteStartTime = scaledTime - (curClipEndTime - noteData.StartTime) * speedRate;
                    scaledStartTimeDict.Add(noteData,scaledNoteStartTime);
                    if (noteData.Type == NoteType.Hold)
                    {
                        //holdEndTime同理
                        float scaledHoldNoteEndTime = scaledTime - (curClipEndTime - noteData.HoldEndTime) * speedRate;
                        scaledHoldEndTimeDict.Add(noteData,scaledHoldNoteEndTime);
                    }

                }
            }
        }
    }

    /// <summary>
    /// 创建视图层物体
    /// </summary>
    public static IView CreateViewObject(NoteData data)
    {
        GameObject go = null;
        switch (data.Type)
        {
            case NoteType.Tap:
                go = Object.Instantiate(GameMgr.Instance.TapPrefab);
                break;
            case NoteType.Hold:
                go = Object.Instantiate(GameMgr.Instance.HoldPrefab);
                break;
            case NoteType.Drag:
                go = Object.Instantiate(GameMgr.Instance.DragPrefab);
                break;
            case NoteType.Click:
                go = Object.Instantiate(GameMgr.Instance.ClickPrefab);
                break;
            case NoteType.Break:
                go = Object.Instantiate(GameMgr.Instance.BreakPrefab);
                break;
        }


        go.transform.SetParent(GameMgr.Instance.viewRoot);
        go.transform.position = GetViewObjectPos(data);
        go.transform.localScale = GetViewObjectScale(data);

        return go.GetComponent<ViewObject>();
    }

    /// <summary>
    /// 根据音符数据获取映射后的视图层位置
    /// </summary>
    private static Vector3 GetViewObjectPos(NoteData data)
    {
        Vector3 pos = default;
        
        //不能直接用StartTime
        pos.y = scaledStartTimeDict[data];
        
        pos.z = -1;
        if (data.Type == NoteType.Break)
        {
            if (Mathf.Abs(data.Pos - (-1)) < 0.01f)
            {
                //左侧break
                pos.x = -2;
            }
            else
            {
                //右侧break
                pos.x = 11;
            }
        }
        else
        {
            pos.x = data.Pos * 10;
        }
        
        return pos;
    }

    /// <summary>
    /// 根据音符数据获取映射后的视图层缩放
    /// </summary>
    private static Vector3 GetViewObjectScale(NoteData data)
    {
        Vector3 scale = Vector3.one;
        if (data.Type == NoteType.Hold)
        {
            //Hold音符需要缩放长度
            float holdLength = scaledHoldEndTimeDict[data] - scaledStartTimeDict[data];
            scale.y = holdLength;
        }

        if (data.Type != NoteType.Break)
        {
            //非Break音符需要缩放宽度
            scale.x = data.Width * 10;
        }
        
        return scale;
    }
}
