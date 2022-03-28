using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CatLrcParser;
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
    //public Text TxtTimer;
    
    public TextAsset LrcAsset;
    
    private InputMapData inputMapData;
    private MusicTimeline timeline;

    public KeyViewController keyViewController;

    private HashSet<KeyCode> pressedKeySet;


    private float deltaTime;
    private float lastTime = 0;

    public float TimelineTime
    {
        get
        {
            if(timeline == null)
            {
                return -0.1f;
            }
            return timeline.Timer;
        }
    }
    
    private void Awake()
    {
        Instance = this;
        inputMapData = InputMapSO.InputMapData;
        pressedKeySet = new HashSet<KeyCode>();
        GameManager.Instance.fullScore = GetFullScore();
    }

    private int GetFullScore()
    {
        int fullScore = 0;
        foreach(var layer in TimelineSo.musicTimelineData.LayerDatas)
        {
            foreach(var clip in layer.ClipDatas)
            {
                foreach(var note in clip.NoteDatas)
                {
                    fullScore += note.GetFullScore();
                }
            }
        }
        return fullScore;
    }

    public float TimeSchedule()
    {
        if(timeline == null)return 1;
        return timeline.Timer/(timeline.Data.Time/1000);
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
        MusicTimelineData data = TimelineSo.musicTimelineData;

        ViewHelper.CalViewTime(data);
        
        timeline = new MusicTimeline(data);
        
        //创建歌词轨道
        Lyric lrc = LrcParser.Parse(LrcAsset.text);
        MusicTimeline.Track lrcTrack = new MusicTimeline.Track(MusicTimeline.TrackType.Lrc);
        for (int i = 0; i < lrc.TimeTagList.Count; i++)
        {
            LrcTimeTag timeTag = lrc.TimeTagList[i];
            LrcNode lrcNode = new LrcNode((float) timeTag.Timestamp.TotalSeconds, timeTag.LyricText);
            lrcTrack.AddNode(lrcNode);
        }
        timeline.AddTrack(lrcTrack);
        
        
        Debug.Log("音乐时间轴创建完毕");
    }
    
    private void Update()
    {
        deltaTime = MusicController.Instance.music.time - lastTime;
        lastTime = MusicController.Instance.music.time;
        CheckKeyboardInput();
        timeline?.OnUpdate(deltaTime);
        //RefreshTimer(timeline.Timer);
    }
    
    /// <summary>
    /// 检查键盘输入
    /// </summary>
    private void CheckKeyboardInput()
    {
        if(GameManager.Instance.isAutoMode)return;
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
