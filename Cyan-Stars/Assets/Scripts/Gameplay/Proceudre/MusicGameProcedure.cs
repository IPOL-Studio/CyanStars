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
using CyanStars.Gameplay.Event;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Lrc;
using CyanStars.Gameplay.MapData;
using CyanStars.Gameplay.Music;
using CyanStars.Gameplay.Note;
using CyanStars.Gameplay.PromptTone;
using CyanStars.Gameplay.UI;
using UnityEngine;
using UInput = UnityEngine.Input;

namespace CyanStars.Gameplay.Procedure
{
    /// <summary>
    /// 音游流程
    /// </summary>
    [ProcedureState]
    public class MusicGameProcedure : BaseState
    {
        //----音游数据模块--------
        private MusicGameModule dataModule = GameRoot.GetDataModule<MusicGameModule>();

        //----场景物体与组件--------
        private GameObject sceneRoot;
        private AudioSource audioSource;
        private KeyViewController keyViewController;

        //----音游相关数据--------
        private InputMapData inputMapData;
        private MapManifest mapManifest;
        private MapTimelineData timelineData;
        private string lrcText;
        private AudioClip music;

        //----时间轴相关对象--------
        private Timeline timeline;
        private NoteTrack noteTrack;
        private List<NoteData> LinearNoteData = new List<NoteData>();

        //----流程逻辑相关--------
        private HashSet<KeyCode> pressedKeySet = new HashSet<KeyCode>();
        private float lastTime = -float.Epsilon;
        private bool isStartGame = false;


        public override async void OnEnter()
        {
            //监听事件
            GameRoot.Event.AddListener(EventConst.MusicGameStartEvent, StartMusicGame);

            //打开游戏场景
            bool success = await GameRoot.Asset.AwaitLoadScene("Assets/BundleRes/Scenes/Dark.unity");

            if (success)
            {
                //获取场景物体与组件
                GetSceneObj();

                //加载数据文件
                await LoadDataFile();

                //打开音游UI
                OpenMusicGameUI();
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
            sceneRoot = null;
            audioSource = null;
            keyViewController = null;

            inputMapData = null;
            mapManifest = null;
            timelineData = null;
            lrcText = null;
            music = null;

            timeline = null;
            noteTrack = null;
            LinearNoteData.Clear();

            pressedKeySet.Clear();
            lastTime = -float.Epsilon;
            isStartGame = false;

            GameRoot.Event.RemoveListener(EventConst.MusicGameStartEvent, StartMusicGame);
            GameRoot.Asset.UnloadScene("Assets/BundleRes/Scenes/Dark.unity");
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartMusicGame(object sender, EventArgs args)
        {
            isStartGame = true;
            CreateTimeline();
        }

        /// <summary>
        /// 获取场景物体与组件
        /// </summary>
        private void GetSceneObj()
        {
            sceneRoot = GameObject.Find("SceneRoot");
            ViewHelper.ViewRoot = sceneRoot.transform.Find("ViewRoot");
            ViewHelper.EffectRoot = sceneRoot.transform.Find("EffectRoot");
            audioSource = sceneRoot.GetComponent<AudioSource>();
            keyViewController = sceneRoot.transform.Find("KeyViewController").GetComponent<KeyViewController>();
        }

        /// <summary>
        /// 加载数据文件
        /// </summary>
        private async Task LoadDataFile()
        {
            //输入映射数据
            InputMapSO inputMapSo = await GameRoot.Asset.AwaitLoadAsset<InputMapSO>(dataModule.InputMapDataName, sceneRoot);
            inputMapData = inputMapSo.InputMapData;

            //谱面清单
            mapManifest = dataModule.GetMap(dataModule.MapIndex);

            //时间轴数据
            MapTimelineDataSO timelineDataSo = await GameRoot.Asset.AwaitLoadAsset<MapTimelineDataSO>(mapManifest.TimelineFileName, sceneRoot);
            timelineData = timelineDataSo.Data;
            timelineData.Time = mapManifest.Time; //在这里赋值时间轴数据的Time字段

            dataModule.CalFullScore(timelineData.NoteTrackData);
            ViewHelper.CalViewTime(mapManifest.Time, timelineData.NoteTrackData);

            //歌词
            if (!string.IsNullOrEmpty(mapManifest.LrcFileName))
            {
                TextAsset lrcAsset = await GameRoot.Asset.AwaitLoadAsset<TextAsset>(mapManifest.LrcFileName, sceneRoot);
                lrcText = lrcAsset.text;
            }

            //音乐
            if (!string.IsNullOrEmpty(mapManifest.MusicFileName))
            {
                music = await GameRoot.Asset.AwaitLoadAsset<AudioClip>(mapManifest.MusicFileName, sceneRoot);
            }
        }

        /// <summary>
        /// 打开音游UI
        /// </summary>
        private void OpenMusicGameUI()
        {
            GameRoot.UI.OpenUI<MusicGameMainPanel>(null);
            GameRoot.UI.OpenUI<MusicGame3DUIPanel>(null);
        }

        /// <summary>
        /// 创建时间轴
        /// </summary>
        private void CreateTimeline()
        {
            MapTimelineData data = timelineData;

            Timeline timeline = new Timeline(mapManifest.Time / 1000f);
            timeline.OnStop += () =>
            {
                this.timeline = null;
                isStartGame = false;
                lastTime = -float.Epsilon;
                Debug.Log("时间轴结束");
            };

            //添加音符轨道
            TrackHelper.CreateBuilder<NoteTrack, MapTimelineData>()
                .AddClips(1, data, NoteTrack.ClipCreator)
                .PostProcess(track => noteTrack = track)
                .Build()
                .AddToTimeline(timeline);

            if (!string.IsNullOrEmpty(lrcText))
            {
                //添加歌词轨道
                Lyric lrc = LrcParser.Parse(lrcText);
                TrackHelper.CreateBuilder_I<LrcTrack, LrcTimeTag>()
                    .AddClips(lrc.TimeTagList, LrcTrack.ClipCreator)
                    .SortClip()
                    .PostProcess(track => { track.TxtLrc = GameRoot.UI.GetUI<MusicGameMainPanel>().TxtLrc; })
                    .Build()
                    .AddToTimeline(timeline);
            }

            //添加相机轨道
            TrackHelper.CreateBuilder<CameraTrack, CameraTrackData>()
                .AddClips(data.CameraTrackData, CameraTrack.ClipCreator)
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
            TrackHelper.CreateBuilder_I<PromptToneTrack, NoteData>()
                .AddClips(LinearNoteData, PromptToneTrack.ClipCreator)
                .SortClip()
                .PostProcess(track => track.audioSource = audioSource)
                .Build()
                .AddToTimeline(timeline);

            //添加特效轨道
            TrackHelper.CreateBuilder<EffectTrack, EffectTrackData>()
                .AddClips(data.EffectTrackData, EffectTrack.ClipCreator)
                .SortClip()
                .PostProcess(track =>
                {
                    track.BPM = data.EffectTrackData.BPM;
                    track.EffectNames = dataModule.EffectNames;
                    track.EffectParent = ViewHelper.EffectRoot;
                    track.ImgFrame = GameRoot.UI.GetUI<MusicGameMainPanel>().ImgFrame;
                })
                .Build()
                .AddToTimeline(timeline);

            this.timeline = timeline;
            dataModule.RunningTimeline = timeline;
            Debug.Log("时间轴创建完毕");
        }

        private void GetLinearNoteData()
        {
            LinearNoteData.Clear();
            foreach (NoteLayerData layer in timelineData.NoteTrackData.LayerDatas)
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
            if (noteTrack == null)
            {
                Debug.LogError("noteTrack为null");
                return;
            }

            noteTrack.OnInput(inputType, item);
        }
    }
}