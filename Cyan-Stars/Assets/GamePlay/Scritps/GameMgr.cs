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
    
    //public Button BtnStart;
    //public Text TxtTimer;
    
    private InputMapData inputMapData;
    private MusicTimeline timeline;

    public KeyViewController keyViewController;

    private HashSet<KeyCode> pressedKeySet;

    private void Awake()
    {
        Instance = this;
        inputMapData = InputMapSO.InputMapData;
        pressedKeySet = new HashSet<KeyCode>();
    }

    public float TimeSchedule()
    {
        return timeline.timer/timeline.data.Time;
    }
    
    private void Start()
    {
        MusicTimelineData data = TimelineSo.musicTimelineData;

        ViewHelper.CalViewTime(data);
        
        timeline = new MusicTimeline(data);
        Debug.Log("音乐时间轴创建完毕");
        //BtnStart.onClick.AddListener(OnBtnStartClick);
    }
    /*
    /// <summary>
    /// 点击开始按钮
    /// </summary>
    private void OnBtnStartClick()
    {
        MusicTimelineData data = TimelineSo.musicTimelineData;

        ViewHelper.CalViewTime(data);
        
        timeline = new MusicTimeline(data);
        Debug.Log("音乐时间轴创建完毕");
    }
    */
    
    private void Update()
    {
        CheckKeyboardInput();
        timeline?.OnUpdate(Time.deltaTime);
    }
    
    /// <summary>
    /// 检查键盘输入
    /// </summary>
    private void CheckKeyboardInput()
    {
        for (int i = 0; i < inputMapData.Items.Count; i++)
        {
            InputMapData.Item item = inputMapData.Items[i];
            if (Input.GetKeyDown(item.key))
            {
                pressedKeySet.Add(item.key);
                ReceiveInput(InputType.Down,item);
                keyViewController.KeyDown(item);
                continue;
            }
            
            if (Input.GetKey(item.key))
            {
                ReceiveInput(InputType.Press,item);
                continue;
            }

            if (pressedKeySet.Remove(item.key))
            {
                ReceiveInput(InputType.Up,item);
                keyViewController.KeyUp(item);
            }
        }
    }

    /// <summary>
    /// 接收输入
    /// </summary>
    public void ReceiveInput(InputType inputType, InputMapData.Item item)
    {
        timeline?.OnInput(inputType,item);
    }
    /*
    public void RefreshTimer(float time)
    {
        TxtTimer.text = time.ToString("0.00");
    }
    */

    public void TimelineEnd()
    {
        timeline = null;
    }
}
