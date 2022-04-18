using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CatLrcParser;
using CyanStars.Framework;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.UI;
using CyanStars.Gameplay.Lrc;
using CyanStars.Gameplay.Note;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Music;
using CyanStars.Gameplay.Effect;
using CyanStars.Gameplay.Camera;
using CyanStars.Gameplay.Evaluate;
using CyanStars.Gameplay.PromptTone;
using CyanStars.Gameplay.Data;
using UInput = UnityEngine.Input;

namespace CyanStars.Gameplay
{
    /// <summary>
    /// 游戏管理器
    /// </summary>
    public class GameManager : SingletonMono<GameManager>
    {
        public InputMapSO InputMapSO;
        public MusicGameDataSO MusicGameDataSo;
      
        
        [Header("特效边框")]
        public Image Frame;
        [Header("特效种类")] 
        public List<GameObject> EffectList;
        
        public UnityEngine.Camera MainCamera;
        public AudioSource AudioSource;
        public Transform viewRoot;
        public GameObject TapPrefab;
        public GameObject HoldPrefab;
        public GameObject DragPrefab;
        public GameObject ClickPrefab;
        public GameObject BreakPrefab;
        public Transform EffectParent;
        
        public Button BtnStart;


        public TextAsset LrcAsset;
        public AudioClip Music;
        private InputMapData inputMapData;
        private MusicGameData MusicGameData;
        private Timeline timeline;
        private NoteTrack noteTrack;

        public KeyViewController keyViewController;

        private HashSet<KeyCode> pressedKeySet;


        private float deltaTime;
        private float lastTime = -float.Epsilon;

        public float TimelineTime
        {
            get
            {
                if (timeline == null)
                {
                    return -0.1f;
                }

                return timeline.CurrentTime;
            }
        }

        [Header("-----UI----------")] public List<PlayingUI> playingUIList = new List<PlayingUI>(); //游戏UI

        [Header("-----游戏数据-----")] [Header("1.Combo数量")]
        public int combo = 0; //Combo数量

        [Header("2.分数")] public float score = 0; //分数
        [Header("3.评分")] public EvaluateType grade; //评分
        [Header("4.当前精准度")] public float currentDeviation = 0; //当前精准度
        [Header("5.各个音符的偏移")] public List<float> deviationList = new List<float>(); //各个音符的偏移
        [Header("6.理论最高分")] public float maxScore = 0; //理论最高分
        [Header("7.各个评分数量")] public int excatNum = 0;
        public int greatNum = 0;
        public int rightNum = 0;
        public int badNum = 0;
        public int missNum = 0;
        [Header("8.当前歌词")] public string curLrcText;
        [Header("9.全谱总分")] public float fullScore = 0;

        [Header("-----游戏模式-----")] [Header("AutoMode")]
        public bool isAutoMode = false; //是否为自动模式

        private List<NoteData> LinearNoteData = new List<NoteData>();

        protected override void OnAwake()
        {
            Application.targetFrameRate = 60;
            
            inputMapData = InputMapSO.InputMapData;
            pressedKeySet = new HashSet<KeyCode>();

            //CameraControllerSo.keyFrames.Sort((x, y) => x.time.CompareTo(y.time));
            MusicGameData = MusicGameDataSo.Data;
            fullScore = GetFullScore();
        }


        private void Start()
        {
            BtnStart.onClick.AddListener(OnBtnStartClick);
        }

        private void Update()
        {
            if (timeline != null)
            {
                CheckKeyboardInput();

                deltaTime = AudioSource.time - lastTime;
                lastTime = AudioSource.time;

                timeline.OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 点击开始按钮
        /// </summary>
        private void OnBtnStartClick()
        {
            MusicGameData data = MusicGameData;
            ViewHelper.CalViewTime(data.Time, data.NoteTrackData);

            LrcAsset = Resources.Load<TextAsset>(data.LrcFileName);
            Lyric lrc = LrcParser.Parse(LrcAsset.text);

            timeline = new Timeline(data.Time / 1000f);
            timeline.OnStop += () => { timeline = null; };

            //添加音符轨道
            TrackHelper.CreateBuilder<NoteTrack, MusicGameData>()
                .AddClips(1, data, NoteTrack.ClipCreator)
                .PostProcess(track => noteTrack = track)
                .Build()
                .AddToTimeline(timeline);

            //添加歌词轨道
            TrackHelper.CreateBuilder<LrcTrack, IList<LrcTimeTag>>()
                .AddClips(lrc.TimeTagList.Count, lrc.TimeTagList, LrcTrack.ClipCreator)
                .SortClip()
                .Build()
                .AddToTimeline(timeline);

            //添加相机轨道
            TrackHelper.CreateBuilder<CameraTrack, CameraTrackData>()
                .AddClips(data.CameraTrackData.KeyFrames.Count, data.CameraTrackData, CameraTrack.ClipCreator)
                .SortClip()
                .PostProcess(track =>
                {
                    track.DefaultCameraPos = data.CameraTrackData.DefaultPosition;
                    track.oldRot = data.CameraTrackData.DefaultRotation;
                    track.CameraTrans = MainCamera.transform;
                })
                .Build()
                .AddToTimeline(timeline);

            //添加音乐轨道
            Music = Resources.Load<AudioClip>(data.MusicFileName);
            TrackHelper.CreateBuilder<MusicTrack, AudioClip>()
                .AddClips(1, Music, MusicTrack.ClipCreator)
                .PostProcess(track => track.audioSource = AudioSource)
                .Build()
                .AddToTimeline(timeline);

            //添加提示音轨道
            TrackHelper.CreateBuilder<PromptToneTrack, IList<NoteData>>()
                .AddClips(LinearNoteData.Count, LinearNoteData, PromptToneTrack.ClipCreator)
                .PostProcess(track => track.audioSource = AudioSource)
                .Build()
                .AddToTimeline(timeline);

            //添加特效轨道
            TrackHelper.CreateBuilder<EffectTrack, EffectTrackData>()
                .AddClips(data.EffectTrackData.KeyFrames.Count,data.EffectTrackData,EffectTrack.ClipCreator)
                .SortClip()
                .PostProcess(track =>
                {
                    track.BPM = data.EffectTrackData.BPM;
                    track.EffectPrefabs = EffectList;
                    track.EffectParent = EffectParent;
                    track.Frame = Frame;
                })
                .Build()
                .AddToTimeline(timeline);

            Debug.Log("时间轴创建完毕");
        }

        private float GetFullScore()
        {
            float fullScore = 0;
            foreach (var layer in MusicGameData.NoteTrackData.LayerDatas)
            {
                foreach (var timeAxis in layer.TimeAxisDatas)
                {
                    foreach (var note in timeAxis.NoteDatas)
                    {
                        fullScore += note.GetFullScore();
                        LinearNoteData.Add(note);
                    }
                }
            }

            return fullScore;
        }


        public float TimeSchedule()
        {
            if (timeline == null) return 1;
            return timeline.CurrentTime / timeline.Length;
        }


        /// <summary>
        /// 检查键盘输入
        /// </summary>
        private void CheckKeyboardInput()
        {
            if (isAutoMode) return;

            for (int i = 0; i < inputMapData.Items.Count; i++)
            {
                InputMapData.Item item = inputMapData.Items[i];
                if (UInput.GetKeyDown(item.key))
                {
                    pressedKeySet.Add(item.key);
                    ReceiveInput(InputType.Down, item);
                    keyViewController.KeyDown(item);
                    continue;
                }

                if (UInput.GetKey(item.key))
                {
                    ReceiveInput(InputType.Press, item);
                    continue;
                }

                if (pressedKeySet.Remove(item.key))
                {
                    ReceiveInput(InputType.Up, item);
                    keyViewController.KeyUp(item);
                }
            }
        }

        /// <summary>
        /// 接收输入
        /// </summary>
        public void ReceiveInput(InputType inputType, InputMapData.Item item)
        {
            if(noteTrack == null)
            {
                Debug.LogError("noteTrack为null");
                return;
            }
            noteTrack.OnInput(inputType, item);
        }


        public void RefreshPlayingUI(int combo, float score, string grade)
        {
            foreach (var item in playingUIList)
            {
                item.Refresh(combo, score, grade, -1);
            }
        }

        public void RefreshData(int addCombo, float addScore, EvaluateType grade, float currentDeviation)
        {
            if (addCombo < 0)
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
                EvaluateType.Bad => badNum++,
                EvaluateType.Miss => missNum++,
                _ => throw new System.NotImplementedException()
            };


            if (currentDeviation < 10000)
            {
                this.currentDeviation = currentDeviation;
                deviationList.Add(currentDeviation);
            }

            RefreshPlayingUI(combo, score, grade.ToString());
        }
    }
}