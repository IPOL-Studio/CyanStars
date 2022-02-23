using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏管理器
/// </summary>
public class GameMgr : MonoBehaviour
{
        
    public static GameMgr Instance;
    
    public TimelineSO SO;
    public Transform ViewRoot;
    
    public GameObject TapPrefab;
    public GameObject HoldPrefab;
    public GameObject DragPrefab;
    public GameObject ClickPrefab;
    
    //public Button BtnStart;
    //public Text TxtTimer;
    
    private MusicTimeline timeline;

    private Dictionary<KeyCode, int> keyMap;

    private void Awake()
    {
        Instance = this;

        keyMap = new Dictionary<KeyCode, int>();
        /*
        keyMap.Add(KeyCode.Q, 1);
        keyMap.Add(KeyCode.A, 1);
        keyMap.Add(KeyCode.Z, 1);
        keyMap.Add(KeyCode.W, 2);
        keyMap.Add(KeyCode.S, 2);
        keyMap.Add(KeyCode.X, 2);
        keyMap.Add(KeyCode.E, 3);
        keyMap.Add(KeyCode.D, 3);
        keyMap.Add(KeyCode.C, 3);
        keyMap.Add(KeyCode.R, 4);
        keyMap.Add(KeyCode.F, 4);
        keyMap.Add(KeyCode.V, 4);
        keyMap.Add(KeyCode.T, 5);
        keyMap.Add(KeyCode.G, 5);
        keyMap.Add(KeyCode.B, 5);
        keyMap.Add(KeyCode.Y, 6);
        keyMap.Add(KeyCode.H, 6);
        keyMap.Add(KeyCode.N, 6);
        keyMap.Add(KeyCode.U, 7);
        keyMap.Add(KeyCode.J, 7);
        keyMap.Add(KeyCode.M, 7);
        keyMap.Add(KeyCode.I, 8);
        keyMap.Add(KeyCode.K, 8);
        keyMap.Add(KeyCode.Comma, 8);
        keyMap.Add(KeyCode.O, 9);
        keyMap.Add(KeyCode.L, 9);
        keyMap.Add(KeyCode.Period, 9);
        keyMap.Add(KeyCode.P, 10);
        keyMap.Add(KeyCode.Semicolon, 10);
        keyMap.Add(KeyCode.Slash, 10);
        keyMap.Add(KeyCode.LeftBracket, 11);
        keyMap.Add(KeyCode.Quote, 11);
        keyMap.Add(KeyCode.RightBracket, 12);
        */
    }

    private void Start()
    {
        //BtnStart.onClick.AddListener(OnBtnStartClick);
        timeline = new MusicTimeline(SO.TimelineData);
        Debug.Log("音乐时间轴创建完毕");
    }

    private void Update()
    { 
        CheckInput();
        
        timeline?.OnUpdate(Time.deltaTime);
 
    }

    /*
    private void OnBtnStartClick()
    {
        timeline = new MusicTimeline(SO.TimelineData);
        Debug.Log("音乐时间轴创建完毕");
    }
    */

    /// <summary>
    /// 检查键盘输入
    /// </summary>
    private void CheckInput()
    {
        foreach (KeyValuePair<KeyCode,int> pair in keyMap)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                timeline?.OnKeyDown(pair.Value);
            }

            if (Input.GetKey(pair.Key))
            {
                timeline?.OnKeyPress(pair.Value);
            }

            if (Input.GetKeyUp(pair.Key))
            {
                timeline?.OnKeyUp(pair.Value);
            }
        }
    }
    /*
    /// <summary>
    /// 刷新计时器文本
    /// </summary>
    public void RefreshTxtTimer(float timer)
    {
        TxtTimer.text = timer.ToString("0.00");
    }
    */

    public float GetCurTimelineTime()
    {
        return timeline.Timer;
    }
    public float TimeSchedule()
    {
        if(timeline != null && SO != null)return timeline.Timer / SO.TimelineData.Time;
        return 1;
    }
    /// <summary>
    /// 时间轴结束
    /// </summary>
    public void TimelineEnd()
    {
        timeline = null;
    }

    /// <summary>
    /// 创建视图层物体
    /// </summary>
    /// <returns></returns>
    public IView CreateView(int trackIndex,NoteData data)
    {
        GameObject go = null;
        switch (data.Type)
        {
            case NoteType.Tap:
                go = Instantiate(TapPrefab);
                break;
            
            case NoteType.Hold:
                go = Instantiate(HoldPrefab);
                go.transform.localScale = new Vector3(go.transform.localScale.x, go.transform.localScale.y
                , data.HoldLength * go.transform.localScale.z);
                break;
            case NoteType.Drag:
                go = Instantiate(DragPrefab);
                break;
            case NoteType.Click:
                go = Instantiate(ClickPrefab);
                break;

        }
        
        go.transform.SetParent(ViewRoot);
        go.transform.position = GetViewStartPos(trackIndex, data.TimePoint);
        go.name = $"Note:{data.Type}-{trackIndex}-{data.TimePoint}";
        return go.GetComponent<View>();
    }
    
    /// <summary>
    /// 获取视图层物体的初始位置
    /// </summary>
    private Vector3 GetViewStartPos(int trackIndex,float startTime)
    {
        Vector3 pos = default;
        pos.z = 100;
        pos.x = trackIndex * 2.5f - 16.5f;
        /*
        switch (trackIndex)
        {
            case 1:
                pos.x = 0;
                break;
            case 2:
                pos.x = -1.5f;
                break;
            case 3:
                pos.x = -3;
                break;
        }
        */
        pos.z = (startTime + 5) * 2;

        return pos;
    }
}
