using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 游戏管理器
/// </summary>
public class GameMgr : MonoBehaviour
{
    public static GameMgr Instance;

    public MusicTimelineSO TimelineSo;
    public InputMapSO InputMapSO;
    public Transform viewRoot;
    public GameObject TapPrefab;
    public GameObject HoldPrefab;
    public GameObject DragPrefab;
    public GameObject ClickPrefab;
    public GameObject BreakPrefab;
    
    public Button BtnStart;
    public Text TxtTimer;
    
    private InputMapData inputMapData;
    private MusicTimeline timeline;
    
    private void Awake()
    {
        Instance = this;
        inputMapData = InputMapSO.InputMapData;
    }
    
    private void Start()
    {
        BtnStart.onClick.AddListener(OnBtnStartClick);
    }

    /// <summary>
    /// 点击开始按钮
    /// </summary>
    private void OnBtnStartClick()
    {
        timeline = new MusicTimeline(TimelineSo.musicTimelineData);
        Debug.Log("音乐时间轴创建完毕");
    }
    
    private void Update()
    {
        CheckKeybordInput();
        timeline?.OnUpdate(Time.deltaTime);
    }

    /// <summary>
    /// 检查键盘输入
    /// </summary>
    private void CheckKeybordInput()
    {
        for (int i = 0; i < inputMapData.Items.Count; i++)
        {
            InputMapData.Item item = inputMapData.Items[i];

            if (Input.GetKeyDown(item.key))
            {
                ReciveInput(InputType.Down,item);
                continue;
            }
            
            if (Input.GetKey(item.key))
            {
                ReciveInput(InputType.Press,item);
                continue;
            }
            
            if (Input.GetKeyUp(item.key))
            {
                ReciveInput(InputType.Up,item);
            }
        }
    }

    /// <summary>
    /// 接收输入
    /// </summary>
    public void ReciveInput(InputType inputType, InputMapData.Item item)
    {
        timeline?.OnInput(inputType,item);
    }

    /// <summary>
    /// 创建视图层物体
    /// </summary>
    public IView CreateViewObject(NoteData data)
    {
        GameObject go = null;
        switch (data.Type)
        {
            case NoteType.Tap:
                go = Instantiate(TapPrefab);
                break;
            case NoteType.Hold:
                go = Instantiate(HoldPrefab);
                break;
            case NoteType.Drag:
                go = Instantiate(DragPrefab);
                break;
            case NoteType.Click:
                go = Instantiate(ClickPrefab);
                break;
            case NoteType.Break:
                go = Instantiate(BreakPrefab);
                break;
        }
        
        
        go.transform.SetParent(viewRoot);
        go.transform.position = GetViewObjectPos(data);
        go.transform.localScale = GetViewObjectScale(data);
        
        return go.GetComponent<ViewObject>();
    }

    /// <summary>
    /// 根据音符数据获取映射后的视图层位置
    /// </summary>
    private Vector3 GetViewObjectPos(NoteData data)
    {
        Vector3 pos = default;
        pos.y = data.StartTime;
        pos.z = -1;
        if (data.Type == NoteType.Break)
        {
            if (Math.Abs(data.Pos - (-1)) < 0.01f)
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
    private Vector3 GetViewObjectScale(NoteData data)
    {
        Vector3 scale = Vector3.one;
        if (data.Type == NoteType.Hold)
        {
            //Hold音符需要缩放长度
            scale.y = data.HoldLength;
        }

        if (data.Type != NoteType.Break)
        {
            //非Break音符需要缩放宽度
            scale.x = data.Width * 10;
        }
        
        return scale;
    }
    
    public void RefreshTimer(float time)
    {
        TxtTimer.text = time.ToString("0.00");
    }

    public void TimelineEnd()
    {
        timeline = null;
    }
}
