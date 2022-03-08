using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

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
    public const float ViewObjectCreateTime = 100;
    
    /// <summary>
    /// 计算受速率影响的视图层音符开始时间和结束时间，用于视图层物体计算位置和长度
    /// </summary>
    public static void CalViewTime(MusicTimelineData data)
    {
        viewStartTimeDict.Clear();
        viewHoldEndTimeDict.Clear();

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
                    viewStartTimeDict.Add(noteData, scaledNoteStartTime);
                    if (noteData.Type == NoteType.Hold)
                    {
                        //holdEndTime同理
                        float scaledHoldNoteEndTime = scaledTime - (curClipEndTime - noteData.HoldEndTime) * speedRate;
                        viewHoldEndTimeDict.Add(noteData, scaledHoldNoteEndTime);
                    }

                }
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
        go.transform.localEulerAngles = GetViewObjectRotation(data);
        
        var view = go.GetComponent<ViewObject>();

        if (data.Type == NoteType.Hold)
        {
            var startPos = GetViewObjectPosZ(viewStartTimeDict[data]);
            var endPos = viewHoldEndTimeDict[data];
            (view as HoldViewObject).SetMesh(1f, endPos - startPos);
        }

        return view;
    }

    /// <summary>
    /// 根据音符数据获取映射后的视图层位置
    /// </summary>
    private static Vector3 GetViewObjectPos(NoteData data)
    {
        Vector3 pos = default;

        //Y轴位置 一开始就在屏幕内的用scaledStartTimeDict[data]，否则用ViewObjectCreateScaledTime
        pos.z = GetViewObjectPosZ(viewStartTimeDict[data]);

        pos.y = Endpoint.Instance.leftObj.transform.position.y;
        if (data.Type == NoteType.Break)
        {
            if (Mathf.Abs(data.Pos - (-1)) < 0.01f)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetViewObjectPosZ(float time) => Mathf.Min(ViewObjectCreateTime, time);

    /// <summary>
    /// 根据音符数据获取映射后的视图层缩放
    /// </summary>
    private static Vector3 GetViewObjectScale(NoteData data)
    {
        Vector3 scale = Vector3.one;

        if (data.Type != NoteType.Break)
        {
            //非Break音符需要缩放宽度
            scale.x = data.Width * Endpoint.Instance.GetLenth();
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
            if(Mathf.Abs(data.Pos - (-1)) < 0.01f)
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
