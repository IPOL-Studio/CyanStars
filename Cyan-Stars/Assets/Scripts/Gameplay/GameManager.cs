using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CatLrcParser;
using CatTimeline;

/// <summary>
/// 游戏管理器
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
    private Timeline timeline;
    private NoteTrack noteTrack;

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
            return timeline.CurrentTime;
        }
    }
    
    [Header("-----UI----------")]
    public List<PlayingUI> playingUIList = new List<PlayingUI>();//游戏UI

    [Header("-----游戏数据-----")]
    [Header("1.Combo数量")]
    public int combo = 0;//Combo数量
    [Header("2.分数")]
    public float score = 0;//分数
    [Header("3.评分")]
    public EvaluateType grade;//评分
    [Header("4.当前精准度")]
    public float currentDeviation = 0;//当前精准度
    [Header("5.各个音符的偏移")]
    public List<float> deviationList = new List<float>();//各个音符的偏移
    [Header("6.理论最高分")]
    public float maxScore = 0;//理论最高分
    [Header("7.各个评分数量")]
    public int excatNum = 0;
    public int greatNum = 0;
    public int rightNum = 0;
    public int badNum = 0;
    public int missNum = 0;
    [Header("8.当前歌词")] 
    public string curLrcText;
    [Header("9.全谱总分")]
    public float fullScore = 0;
    
    [Header("-----游戏模式-----")]
    [Header("AutoMode")]
    public bool isAutoMode = false;//是否为自动模式
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        Instance = this;
        inputMapData = InputMapSO.InputMapData;
        pressedKeySet = new HashSet<KeyCode>();
        fullScore = GetFullScore();
        RegisterTimelineCreateClipFuncFunc();
    }
    

    private void Start()
    {
        BtnStart.onClick.AddListener(OnBtnStartClick);
    }
    
    private void Update()
    {
        
        CheckKeyboardInput();
        
        deltaTime = MusicController.Instance.music.time - lastTime;
        lastTime = MusicController.Instance.music.time;
        timeline?.Update(deltaTime);

    }
    
    /// <summary>
    /// 点击开始按钮
    /// </summary>
    private void OnBtnStartClick()
    {
        MusicTimelineData data = TimelineSo.musicTimelineData;
        ViewHelper.CalViewTime(data);
        Lyric lrc = LrcParser.Parse(LrcAsset.text);
        
        timeline = new Timeline(data.Time / 1000f);
        timeline.OnStop += () =>
        {
            timeline = null;
        };
        
        //添加音符轨道
        timeline.AddTrack<NoteTrack>(1, data);
        noteTrack = timeline.GetTrack<NoteTrack>();
        
        //添加歌词轨道
        timeline.AddTrack<LrcTrack>(lrc.TimeTagList.Count, lrc.TimeTagList);



        Debug.Log("时间轴创建完毕");
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

    /// <summary>
    /// 注册时间轴创建片段函数
    /// </summary>
    private void RegisterTimelineCreateClipFuncFunc()
    {
        //音符
        Timeline.RegisterCreateClipFunc<NoteTrack>((clipIndex, userdata) =>
        {
            MusicTimelineData data = (MusicTimelineData) userdata;

            NoteClip clip = new NoteClip(0, data.Time / 1000f, data.BaseSpeed, data.SpeedRate);

            for (int i = 0; i < data.LayerDatas.Count; i++)
            {
                LayerData layerData = data.LayerDatas[i];
                NoteLayer layer = new NoteLayer();

                for (int j = 0; j < layerData.ClipDatas.Count; j++)
                {
                    ClipData clipData = layerData.ClipDatas[j];
                    layer.AddTimeSpeedRate(clipData.StartTime / 1000f,clipData.SpeedRate);
                    
                    for (int k = 0; k < clipData.NoteDatas.Count; k++)
                    {
                        NoteData noteData = clipData.NoteDatas[k];

                        BaseNote note = CreateNote(noteData, layer);
                        layer.AddNote(note);

                    }
                }
                
                clip.AddLayer(layer);
            }
            
            return clip;
        });
        
        //歌词
        Timeline.RegisterCreateClipFunc<LrcTrack>((clipIndex, userdata) =>
        {
            List<LrcTimeTag> timeTags = (List<LrcTimeTag>) userdata;
            LrcTimeTag timeTag = timeTags[clipIndex];

            float time = (float) timeTag.Timestamp.TotalSeconds;
            LrcClip clip = new LrcClip(time, time, timeTag.LyricText);
            
            return clip;
        });
    }

    /// <summary>
    /// 根据音符数据创建音符
    /// </summary>
    private BaseNote CreateNote(NoteData noteData, NoteLayer layer)
    {
        BaseNote note = null;
        switch (noteData.Type)
        {
            case NoteType.Tap:
                note = new TapNote();
                break;
            case NoteType.Hold:
                note = new HoldNote();
                break;
            case NoteType.Drag:
                note = new DragNote();
                break;
            case NoteType.Click:
                note = new ClickNote();
                break;
            case NoteType.Break:
                note = new BreakNote();
                break;
        }

        note.Init(noteData, layer);
        return note;
    }


    public float TimeSchedule()
    {
        if(timeline == null)return 1;
        return timeline.CurrentTime/(timeline.Length/1000);
    }
    
    
    /// <summary>
    /// 检查键盘输入
    /// </summary>
    private void CheckKeyboardInput()
    {
        if(isAutoMode)return;
        
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
        noteTrack?.OnInput(inputType,item);
    }

    
    public void RefreshPlayingUI(int combo,float score,string grade)
    {
        foreach(var item in playingUIList)
        {
            item.Refresh(combo,score,grade,-1);
        }
    }
    public void RefreshData(int addCombo,float addScore,EvaluateType grade,float currentDeviation)
    {
        if(addCombo < 0)
        {
            combo = 0;
        }
        else
        {
            this.combo += addCombo;
            this.score += addScore;
        }
        
        this.grade = grade;

        _ = grade switch
        {
            EvaluateType.Exact => excatNum++,
            EvaluateType.Great => greatNum++,
            EvaluateType.Right => rightNum++,
            EvaluateType.Out => rightNum++,
            EvaluateType.Bad   => badNum++,
            EvaluateType.Miss  => missNum++,
            _ => throw new System.NotImplementedException()
        };

        
        if(currentDeviation < 10000)
        {
            this.currentDeviation = currentDeviation;
            deviationList.Add(currentDeviation);
        }    
        RefreshPlayingUI(combo,score,grade.ToString());
    }
}
