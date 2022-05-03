using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatLrcParser;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.FSM;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Camera;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Effect;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Lrc;
using CyanStars.Gameplay.Music;
using CyanStars.Gameplay.Note;
using CyanStars.Gameplay.PromptTone;
using CyanStars.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;
using UInput = UnityEngine.Input;

namespace CyanStars.Gameplay.Procedure
{
    /// <summary>
    /// 音游流程
    /// </summary>
    [ProcedureState]
    public class MusicGameProcedure : BaseState
    {
        private MusicGameModule dataModule;

        private GameObject sceneRoot;
        private AudioSource audioSource;
        private KeyViewController keyViewController;
        private List<PlayingUI> playingUIList = new List<PlayingUI>();
        
        private InputMapData inputMapData;
        private MusicGameData musicGameData;
        private string lrcText;
        private AudioClip music;
        
        private Timeline timeline;
        private NoteTrack noteTrack;
        private List<NoteData> LinearNoteData = new List<NoteData>();

        private HashSet<KeyCode> pressedKeySet = new HashSet<KeyCode>();
        private float lastTime = -float.Epsilon;

        private bool isStartGame = false;
        
        public MusicGameProcedure()
        {
            dataModule = GameRoot.GetDataModule<MusicGameModule>();
            dataModule.procedure = this;
        }
        
        public override async void OnEnter()
        {
            bool success = await GameRoot.Asset.AwaitLoadScene("Assets/BundleRes/Scenes/Dark.unity");

            if (success)
            {
                sceneRoot = GameObject.Find("SceneRoot");
                ViewHelper.ViewRoot = sceneRoot.transform.Find("ViewRoot");
                ViewHelper.EffectRoot = sceneRoot.transform.Find("EffectRoot");
                audioSource = sceneRoot.GetComponent<AudioSource>();
                keyViewController = sceneRoot.transform.Find("KeyViewController").GetComponent<KeyViewController>();
                
                foreach (PlayingUI playingUI in GameObject.FindObjectsOfType<PlayingUI>())
                {
                    playingUIList.Add(playingUI);
                }
                
                await LoadDataFile();
                
                //监听开始和关闭按钮的点击
                Button startBtn = GameObject.Find("PlayingUI/StartButton").GetComponent<Button>();
                startBtn.onClick.AddListener(StartMusicGame);
                Button closeBtn = GameObject.Find("PlayingUI/CloseButton").GetComponent<Button>();
                closeBtn.onClick.AddListener(GameRoot.ChangeProcedure<MainHomeProcedure>);
                
               
            }
        
        }

        public override void OnUpdate(float deltaTime)
        {
            if (isStartGame && timeline != null)
            {
                CheckKeyboardInput();

                deltaTime = audioSource.time - lastTime;
                lastTime = audioSource.time;

                timeline.OnUpdate(deltaTime);
            }
        }

        public override void OnExit()
        {
         
            timeline = null;
            inputMapData = null;
            musicGameData = null;
            timeline = null;
            noteTrack = null;
            LinearNoteData.Clear();
            pressedKeySet.Clear();
            lastTime = -float.Epsilon;
            isStartGame = false;
            
            GameRoot.Asset.UnloadScene("Assets/BundleRes/Scenes/Dark.unity");
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartMusicGame()
        {
            isStartGame = true;
            CreateTimeline();
        }
        
        /// <summary>
        /// 加载数据文件
        /// </summary>
        private async Task LoadDataFile()
        {
            InputMapSO inputMapSo = await GameRoot.Asset.AwaitLoadAsset<InputMapSO>(dataModule.InputMapDataName,sceneRoot);
            inputMapData = inputMapSo.InputMapData;
                
            MusicGameDataSO musicGameDataSo = await GameRoot.Asset.AwaitLoadAsset<MusicGameDataSO>(dataModule.MusicGameDataName,sceneRoot);
            musicGameData = musicGameDataSo.Data;
            dataModule.CalFullScore(musicGameData.NoteTrackData);
            ViewHelper.CalViewTime(musicGameData.Time, musicGameData.NoteTrackData);

            if (!string.IsNullOrEmpty(musicGameData.LrcFileName))
            {
                TextAsset lrcAsset = await GameRoot.Asset.AwaitLoadAsset<TextAsset>(musicGameData.LrcFileName,sceneRoot);
                lrcText = lrcAsset.text;
            }

            if (!string.IsNullOrEmpty(musicGameData.MusicFileName))
            {
                music = await GameRoot.Asset.AwaitLoadAsset<AudioClip>(musicGameData.MusicFileName,sceneRoot);
            }
           
         
        }

        /// <summary>
        /// 创建时间轴
        /// </summary>
        private void CreateTimeline()
        {
            MusicGameData data = musicGameData;
            
            Timeline timeline = new Timeline(data.Time / 1000f);
            timeline.OnStop += () =>
            {
                this.timeline = null;
                isStartGame = false;
                lastTime = -float.Epsilon;
                Debug.Log("时间轴结束");
            };

            //添加音符轨道
            TrackHelper.CreateBuilder<NoteTrack, MusicGameData>()
                .AddClips(1, data, NoteTrack.ClipCreator)
                .PostProcess(track => noteTrack = track)
                .Build()
                .AddToTimeline(timeline);

            if (!string.IsNullOrEmpty(lrcText))
            {
                //添加歌词轨道
                Lyric lrc = LrcParser.Parse(lrcText);
                TrackHelper.CreateBuilder<LrcTrack, IList<LrcTimeTag>>()
                    .AddClips(lrc.TimeTagList.Count, lrc.TimeTagList, LrcTrack.ClipCreator)
                    .SortClip()
                    .Build()
                    .AddToTimeline(timeline);
               
            }

            //添加相机轨道
            TrackHelper.CreateBuilder<CameraTrack, CameraTrackData>()
                .AddClips(data.CameraTrackData.KeyFrames.Count, data.CameraTrackData, CameraTrack.ClipCreator)
                .SortClip()
                .PostProcess(track =>
                {
                    track.DefaultCameraPos = data.CameraTrackData.DefaultPosition;
                    track.oldRot = data.CameraTrackData.DefaultRotation;
                    track.CameraTrans = UnityEngine.Camera.main.transform;
                })
                .Build()
                .AddToTimeline(timeline);

            //添加音乐轨道
            if (music)
            {
                TrackHelper.CreateBuilder<MusicTrack, AudioClip>()
                    .AddClips(1, music, MusicTrack.ClipCreator)
                    .PostProcess(track => track.audioSource = audioSource)
                    .Build()
                    .AddToTimeline(timeline);
            }
            

            //添加提示音轨道
            GetLinearNoteData();
            TrackHelper.CreateBuilder<PromptToneTrack, IList<NoteData>>()
                .AddClips(LinearNoteData.Count, LinearNoteData, PromptToneTrack.ClipCreator)
                .PostProcess(track => track.audioSource = audioSource)
                .Build()
                .AddToTimeline(timeline);

            //添加特效轨道
            TrackHelper.CreateBuilder<EffectTrack, EffectTrackData>()
                .AddClips(data.EffectTrackData.KeyFrames.Count,data.EffectTrackData,EffectTrack.ClipCreator)
                .SortClip()
                .PostProcess(track =>
                {
                    track.BPM = data.EffectTrackData.BPM;
                    track.EffectNames = dataModule.EffectNames;
                    track.EffectParent = ViewHelper.EffectRoot;
                    track.Frame = GameObject.Find("PlayingUI/Frame").GetComponent<Image>();
                })
                .Build()
                .AddToTimeline(timeline);

            this.timeline = timeline;
            Debug.Log("时间轴创建完毕");
        }

        private void GetLinearNoteData()
        {
            LinearNoteData.Clear();
            foreach (NoteLayerData layer in musicGameData.NoteTrackData.LayerDatas)
            {
                foreach (NoteTimeAxisData timeAxis in layer.TimeAxisDatas)
                {
                    foreach (NoteData note in timeAxis.NoteDatas)
                    {
                       LinearNoteData.Add(note);
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查键盘输入
        /// </summary>
        private void CheckKeyboardInput()
        {
            if (dataModule.IsAutoMode) return;

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
        private void ReceiveInput(InputType inputType, InputMapData.Item item)
        {
            if(noteTrack == null)
            {
                Debug.LogError("noteTrack为null");
                return;
            }
            noteTrack.OnInput(inputType, item);
        }
        
        public float TimeSchedule()
        {
            if (timeline == null) return 1;
            return timeline.CurrentTime / timeline.Length;
        }
        
        /// <summary>
        /// 刷新PlayingUI
        /// </summary>
        public void RefreshPlayingUI(int combo, float score, string grade)
        {
            foreach (var item in playingUIList)
            {
                item.Refresh(combo, score, grade, -1);
            }
        }
    }
}

