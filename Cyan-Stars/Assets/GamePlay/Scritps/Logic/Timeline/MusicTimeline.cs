using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐时间轴
/// </summary>
public class MusicTimeline
{
    private TimelineData data;

    private List<MusicTrack> tracks = new List<MusicTrack>();

    public float Timer;
    
    public MusicTimeline(TimelineData data)
    {
        this.data = data;
        CreateTracks();
    }

    /// <summary>
    /// 创建轨道
    /// </summary>
    private void CreateTracks()
    {
        List<TrackData> trackDatas = data.TrackDatas;
        for (int i = 0; i < trackDatas.Count; i++)
        {
            MusicTrack track = new MusicTrack(i + 1,trackDatas[i]);
            tracks.Add(track);
        }
    }
    
    public void OnUpdate(float deltaTime)
    {
        deltaTime *= data.SpeedRate;
        Timer += deltaTime;

        GameMgr.Instance.RefreshTxtTimer(Timer);
        
        if (Timer >= data.Time)
        {
            //时间轴跑完了
            GameMgr.Instance.TimelineEnd();
            return;;
        }
        
        
        for (int i = 0; i < tracks.Count; i++)
        {
            tracks[i].OnUpdate(deltaTime);
        }
    }
    
    public void OnKeyDown(int trackIndex)
    {
        tracks[trackIndex - 1].OnKeyDown();
    }
    
    public void OnKeyUp(int trackIndex)
    {
        tracks[trackIndex - 1].OnKeyUp();
    }

    public void OnKeyPress(int trackIndex)
    {
        tracks[trackIndex - 1].OnKeyPress();
    }


}
