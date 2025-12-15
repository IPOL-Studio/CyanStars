using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.FSM;
using CyanStars.Framework.Logging;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Base;
using CyanStars.Chart;
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
        //  --- --- 音游数据模块 --- ---
        private ChartModule chartModule = GameRoot.GetDataModule<ChartModule>();
        private MusicGamePlayingDataModule playingDataModule = GameRoot.GetDataModule<MusicGamePlayingDataModule>();
        private MusicGameTrackModule trackModule = GameRoot.GetDataModule<MusicGameTrackModule>();

        //  --- --- 音游相关数据 --- ---
        private RuntimeChartPack runtimeChartPack;
        private ChartData chartData;

        private PromptToneCollection promptToneCollection;

        private MusicClipData musicClipData;

        //  --- --- 音游设置模块 --- ---
        private MusicGameSettingsModule settingsModule = GameRoot.GetDataModule<MusicGameSettingsModule>();

        //  --- --- 音游场景模块 --- ---
        private MusicGameSceneModule sceneModule = GameRoot.GetDataModule<MusicGameSceneModule>();
        private MusicGameSceneInfo currentSceneInfo;

        //  --- --- 场景物体与组件 --- ---
        private SceneHandler sceneHandler;
        private GameObject sceneRoot;
        private Transform sceneCameraTrans;
        private AudioSource audioSource;

        //  --- --- 场景后处理特效相关 --- ---
        private Band band;


        //  --- --- 时间轴相关对象 --- ---
        private Timeline timeline;

        //  --- --- 流程逻辑相关 --- ---
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
            var sceneHandler = await GameRoot.Asset.LoadSceneAsync(currentSceneInfo.ScenePath);

            if (sceneHandler.IsValid && sceneHandler.IsSuccess)
            {
                this.sceneHandler = sceneHandler;

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

            runtimeChartPack = null;
            chartData = null;
            musicClipData = null;

            // timelineData = null;
            // lrcText = null;

            timeline = null;

            inputReceiver = null;
            lastTime = -float.Epsilon;

            preDistanceBarChangedCount = 0;

            GameRoot.Event.RemoveListener(EventConst.MusicGameStartEvent, OnMusicGameStart);
            GameRoot.Event.RemoveListener(EventConst.MusicGamePauseEvent, OnMusicGamePause);
            GameRoot.Event.RemoveListener(EventConst.MusicGameResumeEvent, OnMusicGameResume);
            GameRoot.Event.RemoveListener(EventConst.MusicGameExitEvent, OnMusicGameExit);

            CloseMusicGameUI();

            GameRoot.Asset.UnloadScene(sceneHandler);
            currentSceneInfo = null;
            sceneHandler = default;
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

            playingDataModule.ResetPlayingData();
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
            // 输入映射数据
            var inputMapSoHandler = await GameRoot.Asset.LoadAssetAsync<InputMapSO>(playingDataModule.InputMapDataName).BindTo(sceneRoot);
            InputMapData inputMapData = inputMapSoHandler.Asset.InputMapData;

            if (!playingDataModule.IsAutoMode)
            {
                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    //使用触屏输入
                    TouchInputReceiver touch = new TouchInputReceiver(inputMapData);

                    GameObject prefab = sceneRoot.transform.Find("TouchInputReceiveObj").gameObject;
                    Transform parent = sceneRoot.transform.Find("TouchInputParent");
                    touch.CreateReceiveObj(prefab, parent);

                    inputReceiver = touch;
                }
                else
                {
                    //使用键盘输入
                    inputReceiver = new KeyboardInputReceiver(inputMapData);
                }
            }

            // 谱面
            runtimeChartPack = chartModule.SelectedRuntimeChartPack;
            await chartModule.SelectChartDataAsync(0); // TODO: 根据选择的难度来加载谱面
            if (chartModule.ChartData is null)
            {
                Debug.LogError("谱面加载失败");
            }


            // 音乐
            if (chartModule.SelectedMusicVersionIndex != null)
            {
                MusicVersionData musicVersionData =
                    runtimeChartPack.ChartPackData.MusicVersionDatas[(int)chartModule.SelectedMusicVersionIndex];
                AudioClip music =
                    (await GameRoot.Asset.LoadAssetAsync<AudioClip>(musicVersionData.AudioFilePath)).BindTo(sceneRoot).Asset;
                if (!music)
                {
                    Debug.LogError($"谱包 {runtimeChartPack.ChartPackData.Title} 的音乐加载失败");
                }
                else
                {
                    musicClipData = new MusicClipData(music, musicVersionData.Offset);
                }

                // 时间轴
                playingDataModule.CurTimelineLength = music.length + musicVersionData.Offset / 1000f;
                playingDataModule.CalFullScore(chartModule.ChartData.Notes);
            }


            // //歌词
            // if (!string.IsNullOrEmpty(mapManifest.LrcFileName))
            // {
            //     TextAsset lrcAsset = await GameRoot.Asset.LoadAssetAsync<TextAsset>(mapManifest.LrcFileName, sceneRoot);
            //     lrcText = lrcAsset.text;
            // }
        }

        /// <summary>
        /// 初始化音游相关 Logger
        /// </summary>
        private void InitLogger()
        {
            RuntimeChartPack pack = chartModule.SelectedRuntimeChartPack;
            playingDataModule.InitLogger($"MusicGame - {pack.ChartPackData.Title}");
        }

        /// <summary>
        /// 初始化误差条数据
        /// </summary>
        private void InitDistanceBarData()
        {
            playingDataModule.InitDistanceBarData(settingsModule.EvaluateRange);

            var bandData = new Band.BandData
            {
                Count = playingDataModule.DistanceBarData.Length,
                XSize = 100,
                YSize = 10,
                XOffset = -0.5f,
                YOffset = 0.2f
            };

            if (!Band.TryCreate(bandData, out Band band))
            {
                playingDataModule.Logger.LogError("band 创建失败，可能被其他地方占用");
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
                var clipHandler = await GameRoot.Asset.LoadAssetAsync<AudioClip>(path).BindTo(sceneRoot);
                builtin.Add(name, clipHandler.Asset);
            }

            promptToneCollection = new PromptToneCollection(builtin, playingDataModule.Logger);
        }

        /// <summary>
        /// 打开音游UI
        /// </summary>
        private void OpenMusicGameUI()
        {
            foreach (var type in this.currentSceneInfo.UITypes)
            {
                GameRoot.UI.OpenUIPanelAsync(type);
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
            foreach (KeyValuePair<NoteType, string> pair in playingDataModule.HitEffectPrefabNameDict)
            {
                GameRoot.GameObjectPool.PrewarmAsync(pair.Value, 10, null);
            }
        }

        /// <summary>
        /// 创建时间轴
        /// </summary>
        private void CreateTimeline()
        {
            timeline = new Timeline(playingDataModule.CurTimelineLength);
            timeline.OnStop += StopTimeline;

            // 添加音符轨道
            List<BpmGroupItem> bpmGroup = new List<BpmGroupItem>();
            bpmGroup.AddRange(runtimeChartPack.ChartPackData.BpmGroup);


            NoteTrackData noteTrackData = new NoteTrackData() { BpmGroup = bpmGroup, ClipDataList = new List<ChartData>() { chartData } };
            timeline.AddTrack(noteTrackData, NoteTrack.CreateClipFunc);

            // if (!string.IsNullOrEmpty(lrcText) && settingsModule.EnableLyricTrack)
            // {
            //     //添加歌词轨道
            //     Lyric lrc = LrcParser.Parse(lrcText);
            //     LrcTrackData lrcTrackData = new LrcTrackData { ClipDataList = lrc.TimeTagList };
            //
            //     LrcTrack lrcTrack = timeline.AddTrack(lrcTrackData, LrcTrack.CreateClipFunc);
            //     lrcTrack.TxtLrc = GameRoot.UI.GetUIPanel<MusicGameMainPanel>().TxtLrc;
            // }
            //
            // if (settingsModule.EnableCameraTrack)
            // {
            //     //添加相机轨道
            //     CameraTrack cameraTrack = timeline.AddTrack(data.CameraTrackData, CameraTrack.CreateClipFunc);
            //     cameraTrack.DefaultCameraPos = data.CameraTrackData.DefaultPosition;
            //     cameraTrack.OldRot = data.CameraTrackData.DefaultRotation;
            //     cameraTrack.CameraTrans = sceneCameraTrans.transform;
            // }

            // 添加音乐轨道
            if (musicClipData != null)
            {
                MusicTrackData musicTrackData = new MusicTrackData() { ClipDataList = new List<MusicClipData>() { musicClipData } };

                MusicTrack musicTrack = timeline.AddTrack(musicTrackData, MusicTrack.CreateClipFunc);
                musicTrack.AudioSource = audioSource;
            }

            for (int i = 0; i < chartData.TrackDatas.Count; i++)
            {
                if (trackModule.TryGetTrackLoader(chartData.TrackDatas[i].TrackData.GetType(), out var trackLoader) &&
                    trackLoader.IsEnabled)
                {
                    trackLoader.LoadTrack(timeline, noteTrackData.BpmGroup, chartData, i);
                }
            }

            // // 添加边框闪烁轨道
            // if (settingsModule.EnableFrameTrack)
            // {
            //     foreach (ChartTrackData chartTrackData in chartData.TrackDatas)
            //     {
            //         if (chartTrackData.TrackData.GetType() != typeof(FrameChartTrackData))
            //         {
            //             continue;
            //         }
            //
            //         FrameChartTrackData frameChartTrackData = chartTrackData.TrackData as FrameChartTrackData;
            //         FrameClipData frameClipData = new FrameClipData(frameChartTrackData, chartData.BpmGroups.CalculateTime);
            //         FrameTrackData frameTrackData = new FrameTrackData()
            //         {
            //             ClipDataList = new List<FrameClipData>() { frameClipData }
            //         };
            //
            //         FrameTrack frameTrack = timeline.AddTrack(frameTrackData, FrameTrack.CreateClipFunc);
            //         frameTrack.ImgFrame = GameRoot.UI.GetUIPanel<MusicGameMainPanel>().ImgFrame;
            //
            //         break;
            //     }
            // }

            // //添加提示音轨道
            // GetLinearNoteData();
            // PromptToneTrackData promptToneTrackData = new PromptToneTrackData
            // {
            //     ClipDataList = linearNoteData, PromptToneCollection = promptToneCollection
            // };
            // PromptToneTrack promptToneTrack = timeline.AddTrack(promptToneTrackData, PromptToneTrack.CreateClipFunc);
            // promptToneTrack.AudioSource = audioSource;

            // //添加特效轨道
            // if (settingsModule.EnableEffectTrack)
            // {
            //     EffectTrack effectTrack = timeline.AddTrack(data.EffectTrackData, EffectTrack.CreateClipFunc);
            //     effectTrack.EffectNames = dataModule.EffectNames;
            //     effectTrack.EffectParent = ViewHelper.EffectRoot;
            //     effectTrack.ImgFrame = GameRoot.UI.GetUIPanel<MusicGameMainPanel>().ImgFrame;
            // }

            playingDataModule.RunningTimeline = timeline;

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
                GameRoot.UI.OpenUIPanelAsync<MusicGamePausePanel>();
            }
        }

        /// <summary>
        /// 更新判定误差指示
        /// </summary>
        private void UpdateDistanceBar(float deltaTime)
        {
            var data = playingDataModule.DistanceBarData;
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

            GameRoot.Event.Dispatch(EventConst.MusicGameEndEvent, this, EmptyEventArgs.Create());

            Debug.Log("音游结束");
        }
    }
}
