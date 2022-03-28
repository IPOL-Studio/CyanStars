using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐时间轴
/// </summary>
public partial class MusicTimeline
{
    /// <summary>
    /// 音乐时间轴数据
    /// </summary>
    public MusicTimelineData Data
    {
        get;
    }

    /// <summary>
    /// 计时器
    /// </summary>
    public float Timer
    {
        get;
        private set;
    }

    /// <summary>
    /// 时间轴结束时间
    /// </summary>
    private float endTime;
    
    /// <summary>
    /// 图层列表
    /// </summary>
    private List<Layer> layers = new List<Layer>();

    /// <summary>
    /// 轨道列表
    /// </summary>
    private List<Track> tracks = new List<Track>();

    public MusicTimeline(MusicTimelineData data)
    {
        Data = data;
        endTime = data.Time / 1000f;
        CreateLayers();
    }

    /// <summary>
    /// 创建图层
    /// </summary>
    private void CreateLayers()
    {
        for (int i = 0; i < Data.LayerDatas.Count; i++)
        {
            Layer layer = new Layer(Data.LayerDatas[i]);
            layers.Add(layer);
        }
    }

    /// <summary>
    /// 添加轨道
    /// </summary>
    public void AddTrack(Track track)
    {
        tracks.Add(track);
    }

    public void OnUpdate(float deltaTime)
    {
        //Timer += deltaTime;
        Timer = MusicController.Instance.music.time;
        
        //计算timeline速率
        float timelineSpeedRate = Data.BaseSpeed * Data.SpeedRate;

        if (Timer >= endTime)
        {
            //运行结束
            GameMgr.Instance.TimelineEnd();
            return;;
        }
        
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].OnUpdate(Timer,deltaTime,timelineSpeedRate);
        }

        for (int i = 0; i < tracks.Count; i++)
        {
            tracks[i].OnUpdate(Timer,deltaTime);
        }
    }
    
    public void OnInput(InputType inputType, InputMapData.Item item)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].OnInput(inputType,item);
        }
    }
}
