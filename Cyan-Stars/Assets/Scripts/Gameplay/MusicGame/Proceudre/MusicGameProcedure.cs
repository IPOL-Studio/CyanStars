using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatLrcParser;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.FSM;
using CyanStars.Framework.Logging;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Base;
using CyanStars.Graphics.Band;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游流程
    /// </summary>
    [ProcedureState]
    public class MusicGameProcedure : BaseState
    {
        //----音游数据模块--------
        private MusicGameModule dataModule = GameRoot.GetDataModule<MusicGameModule>();

        //----音游设置模块--------
        private MusicGameSettingsModule settingsModule = GameRoot.GetDataModule<MusicGameSettingsModule>();

        //----音游场景模块--------
        private MusicGameSceneModule sceneModule = GameRoot.GetDataModule<MusicGameSceneModule>();
        private MusicGameSceneInfo currentSceneInfo;

        //----场景物体与组件--------
        private Scene scene;
        private GameObject sceneRoot;
        private Transform sceneCameraTrans;
        private AudioSource audioSource;

        //----场景后处理特效相关--------
        private Band band;

        //----音游相关数据--------
        private MapTimelineData timelineData;
        private string lrcText;
        private AudioClip music;
        private PromptToneCollection promptToneCollection;

        //----时间轴相关对象--------
        private Timeline timeline;
        private List<NoteData> linearNoteData = new List<NoteData>();

        //----流程逻辑相关--------
        private BaseInputReceiver inputReceiver;
        private float lastTime = -float.Epsilon;
        private int preDistanceBarChangedCount;


        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);
            currentSceneInfo = sceneModule.CurrentScene;

            //监听事件
            GameRoot.Event.AddListener(EventConst.MusicGameStartEvent, OnMusicGameStart);
            GameRoot.Event.AddListener(EventConst.MusicGamePauseEvent, OnMusicGamePause);
            GameRoot.Event.AddListener(EventConst.MusicGameResumeEvent, OnMusicGameResume);
            GameRoot.Event.AddListener(EventConst.MusicGameExitEvent, OnMusicGameExit);

            //打开游戏场景
            scene = await GameRoot.Asset.LoadSceneAsync(currentSceneInfo.ScenePath);

            if (scene != default)
            {
                //获取场景物体与组件
                GetSceneObj();

                //初始化音游相关 Logger
                InitLogger();

                //初始化误差条数据
                InitDistanceBarData();

                //加载打击音效
                await LoadPromptTone();

                //加载数据文件
                await LoadDataFile();

                //打开音游UI
                OpenMusicGameUI();

                //预热一些物体
                Prewarm();
            }
        }

        public override void OnUpdate(float deltaTime)
        {

        }


        public override void OnExit()
        {
            GameRoot.MainCamera.gameObject.SetActive(true);

            sceneRoot = null;
            sceneCameraTrans = null;
            audioSource = null;
            promptToneCollection = null;

            band?.Dispose();
            band = null;

            timelineData = null;
            lrcText = null;
            music = null;

            timeline = null;
            linearNoteData.Clear();

            inputReceiver = null;
            lastTime = -float.Epsilon;

            preDistanceBarChangedCount = 0;

            GameRoot.Event.RemoveListener(EventConst.MusicGameStartEvent, OnMusicGameStart);
            GameRoot.Event.RemoveListener(EventConst.MusicGamePauseEvent, OnMusicGamePause);
            GameRoot.Event.RemoveListener(EventConst.MusicGameResumeEvent, OnMusicGameResume);
            GameRoot.Event.RemoveListener(EventConst.MusicGameExitEvent, OnMusicGameExit);

            CloseMusicGameUI();

            GameRoot.Asset.UnloadScene(scene);
            currentSceneInfo = null;
            scene = default;
        }

        /// <summary>
        /// 开始音游
        /// </summary>
        private void OnMusicGameStart(object sender, EventArgs args)
        {
            CreateTimeline();
            inputReceiver?.StartReceive();
        }

        /// <summary>
        /// 暂停音游
        /// </summary>
        private void OnMusicGamePause(object sender, EventArgs e)
        {
            if (timeline == null)
            {
                return;
            }

            audioSource.Pause();
            inputReceiver?.EndReceive();
        }

        /// <summary>
        /// 恢复音游
        /// </summary>
        private void OnMusicGameResume(object sender, EventArgs e)
        {
            if (timeline == null)
            {
                return;
            }

            audioSource.UnPause();
            inputReceiver?.StartReceive();
        }

        /// <summary>
        /// 退出音游
        /// </summary>
        private void OnMusicGameExit(object sender, EventArgs e)
        {
            if (timeline != null)
            {
                //timeline运行中退出 需要先停掉运行中的timeline
                StopTimeline();
            }

            dataModule.ResetPlayingData();
            GameRoot.ChangeProcedure<MainHomeProcedure>();
        }

        /// <summary>
        /// 获取场景物体与组件
        /// </summary>
        private void GetSceneObj()
        {
            sceneRoot = GameObject.Find("SceneRoot");
            ViewHelper.ViewRoot = sceneRoot.transform.Find("ViewRoot");
            ViewHelper.EffectRoot = sceneRoot.transform.Find("EffectRoot");
            sceneCameraTrans = sceneRoot.transform.Find("SceneCamera");
            audioSource = sceneRoot.GetComponent<AudioSource>();
        }

        /// <summary>
        /// 加载数据文件
        /// </summary>
        private async Task LoadDataFile()
        {
            //输入映射数据
            InputMapSO inputMapSo = await GameRoot.Asset.LoadAssetAsync<InputMapSO>(dataModule.InputMapDataName, sceneRoot);
            InputMapData inputMapData = inputMapSo.InputMapData;

            if (!dataModule.IsAutoMode)
            {
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    //使用触屏输入
                    TouchInputReceiver touch = new TouchInputReceiver(inputMapData);

                    GameObject prefab = sceneRoot.transform.Find("TouchInputReceiveObj").gameObject;
                    Transform parent = sceneRoot.transform.Find("TouchInputParent");
                    touch.CreateReceiveObj(prefab,parent);

                    inputReceiver = touch;
                }
                else
                {
                    //使用键盘输入
                    inputReceiver = new KeyboardInputReceiver(inputMapData);
                }
            }


            //谱面清单
            MapManifest mapManifest = dataModule.GetMap(dataModule.MapIndex);

            //时间轴数据
            MapTimelineDataSO timelineDataSo = await GameRoot.Asset.LoadAssetAsync<MapTimelineDataSO>(mapManifest.TimelineFileName, sceneRoot);
            timelineData = timelineDataSo.Data;
            dataModule.CurTimelineLength = timelineData.Length / 1000f;

            dataModule.CalFullScore(timelineData.NoteTrackData);

            //歌词
            if (!string.IsNullOrEmpty(mapManifest.LrcFileName))
            {
                TextAsset lrcAsset = await GameRoot.Asset.LoadAssetAsync<TextAsset>(mapManifest.LrcFileName, sceneRoot);
                lrcText = lrcAsset.text;
            }

            //音乐
            if (!string.IsNullOrEmpty(mapManifest.MusicFileName))
            {
                music = await GameRoot.Asset.LoadAssetAsync<AudioClip>(mapManifest.MusicFileName, sceneRoot);
            }
        }

        /// <summary>
        /// 初始化音游相关 Logger
        /// </summary>
        private void InitLogger()
        {
            var mapData = dataModule.GetMap(dataModule.MapIndex);
            dataModule.InitLogger($"MusicGame - {mapData.Name}");
        }

        /// <summary>
        /// 初始化误差条数据
        /// </summary>
        private void InitDistanceBarData()
        {
            dataModule.InitDistanceBarData(settingsModule.EvaluateRange);

            var bandData = new Band.BandData
            {
                Count = dataModule.DistanceBarData.Length,
                XSize = 100,
                YSize = 10,
                XOffset = 0.5f,
                YOffset = 0.2f
            };

            if (!Band.TryCreate(bandData, out Band band))
            {
                dataModule.Logger.LogError("band 创建失败，可能被其他地方占用");
            }

            this.band = band;
        }

        /// <summary>
        /// 加载打击音效
        /// </summary>
        private async Task LoadPromptTone()
        {
            var builtin = new Dictionary<string, AudioClip>();
            var builtinPath = settingsModule.BuiltInPromptTones;

            foreach (var (name, path) in builtinPath)
            {
                var clip = await GameRoot.Asset.LoadAssetAsync<AudioClip>(path, sceneRoot);
                builtin.Add(name, clip);
            }

            promptToneCollection = new PromptToneCollection(builtin, dataModule.Logger);
        }

        /// <summary>
        /// 打开音游UI
        /// </summary>
        private void OpenMusicGameUI()
        {
            foreach (var type in this.currentSceneInfo.UITypes)
            {
                GameRoot.UI.OpenUIPanel(type, null);
            }
        }

        /// <summary>
        /// 关闭音游UI
        /// </summary>
        private void CloseMusicGameUI()
        {
            foreach (var type in this.currentSceneInfo.UITypes)
            {
                GameRoot.UI.CloseUIPanel(type);
            }
        }

        /// <summary>
        /// 预热一些物体
        /// </summary>
        private void Prewarm()
        {
            foreach (KeyValuePair<NoteType,string> pair in dataModule.HitEffectPrefabNameDict)
            {
                GameRoot.GameObjectPool.Prewarm(pair.Value, 10,null);
            }
        }

        /// <summary>
        /// 创建时间轴
        /// </summary>
        private void CreateTimeline()
        {
            MapTimelineData data = timelineData;

            timeline = new Timeline(data.Length/ 1000f);
            timeline.OnStop += StopTimeline;

            //添加音符轨道
            timeline.AddTrack(data.NoteTrackData, NoteTrack.CreateClipFunc);

            if (!string.IsNullOrEmpty(lrcText) && settingsModule.EnableLyricTrack)
            {
                //添加歌词轨道
                Lyric lrc = LrcParser.Parse(lrcText);
                LrcTrackData lrcTrackData = new LrcTrackData { ClipDataList = lrc.TimeTagList };

                LrcTrack lrcTrack = timeline.AddTrack(lrcTrackData, LrcTrack.CreateClipFunc);
                lrcTrack.TxtLrc = GameRoot.UI.GetUIPanel<MusicGameMainPanel>().TxtLrc;
            }

            if (settingsModule.EnableCameraTrack)
            {
                //添加相机轨道
                CameraTrack cameraTrack = timeline.AddTrack(data.CameraTrackData, CameraTrack.CreateClipFunc);
                cameraTrack.DefaultCameraPos = data.CameraTrackData.DefaultPosition;
                cameraTrack.OldRot = data.CameraTrackData.DefaultRotation;
                cameraTrack.CameraTrans = sceneCameraTrans.transform;
            }

            //添加音乐轨道
            if (music)
            {
                MusicTrackData musicTrackData = new MusicTrackData { ClipDataList = new List<AudioClip>() { music } };

                MusicTrack musicTrack = timeline.AddTrack(musicTrackData, MusicTrack.CreateClipFunc);
                musicTrack.AudioSource = audioSource;
            }

            //添加提示音轨道
            GetLinearNoteData();
            PromptToneTrackData promptToneTrackData = new PromptToneTrackData
            {
                ClipDataList = linearNoteData,
                PromptToneCollection = promptToneCollection
            };
            PromptToneTrack promptToneTrack = timeline.AddTrack(promptToneTrackData, PromptToneTrack.CreateClipFunc);
            promptToneTrack.AudioSource = audioSource;

            //添加特效轨道
            if (settingsModule.EnableEffectTrack)
            {
                EffectTrack effectTrack = timeline.AddTrack(data.EffectTrackData, EffectTrack.CreateClipFunc);
                effectTrack.EffectNames = dataModule.EffectNames;
                effectTrack.EffectParent = ViewHelper.EffectRoot;
                effectTrack.ImgFrame = GameRoot.UI.GetUIPanel<MusicGameMainPanel>().ImgFrame;
            }

            dataModule.RunningTimeline = timeline;

            GameRoot.Timer.UpdateTimer.Add(UpdateTimeline);

            Debug.Log("时间轴创建完毕");
        }

        /// <summary>
        /// 更新时间轴
        /// </summary>
        private void UpdateTimeline(float deltaTime, object userdata)
        {
            float timelineDeltaTime = audioSource.time - lastTime;
            lastTime = audioSource.time;

            timeline.OnUpdate(timelineDeltaTime);
            UpdateDistanceBar(timelineDeltaTime);

            //音游流程中 按下ESC打开暂停
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameRoot.UI.OpenUIPanel<MusicGamePausePanel>(null);
            }
        }

        /// <summary>
        /// 更新判定误差指示
        /// </summary>
        private void UpdateDistanceBar(float deltaTime)
        {
            var data = dataModule.DistanceBarData;
            data.ReduceHeight(deltaTime);
            if (data.IsDataChangedAndSet(ref preDistanceBarChangedCount))
            {
                band.UpdateBand(data.BarHeights);
            }
        }

        /// <summary>
        /// 停止时间轴
        /// </summary>
        private void StopTimeline()
        {
            timeline = null;
            lastTime = -float.Epsilon;
            audioSource.clip = null;

            GameRoot.Timer.UpdateTimer.Remove(UpdateTimeline);
            inputReceiver?.EndReceive();

            GameRoot.Event.Dispatch(EventConst.MusicGameEndEvent, this,EmptyEventArgs.Create());

            Debug.Log("音游结束");
        }

        private void GetLinearNoteData()
        {
            linearNoteData.Clear();
            foreach (NoteLayerData layer in timelineData.NoteTrackData.LayerDatas)
            {
                foreach (NoteTimeAxisData timeAxis in layer.TimeAxisDatas)
                {
                    foreach (NoteData note in timeAxis.NoteDatas)
                    {
                        linearNoteData.Add(note);
                    }
                }
            }
        }

    }
}
