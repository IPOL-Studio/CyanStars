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
    private MusicTimelineData data;

    /// <summary>
    /// 计时器
    /// </summary>
    private float timer;
    
    /// <summary>
    /// 图层列表
    /// </summary>
    private List<Layer> layers = new List<Layer>();
    
    public MusicTimeline(MusicTimelineData data)
    {
        this.data = data;
        CreateLayers();
    }

    /// <summary>
    /// 创建图层
    /// </summary>
    private void CreateLayers()
    {
        for (int i = 0; i < data.LayerDatas.Count; i++)
        {
            Layer layer = new Layer(data.LayerDatas[i]);
            layers.Add(layer);
        }
    }

    public void OnUpdate(float deltaTime)
    {
        //考虑基准速度和谱面的基准速率
        deltaTime *= (data.Speed * data.SpeedRate);
        timer += deltaTime;
        
        GameMgr.Instance.RefreshTimer(timer);

        if (timer >= data.Time)
        {
            //运行结束
            GameMgr.Instance.TimelineEnd();
            return;;
        }
        
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].OnUpdate(deltaTime,timer);
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
