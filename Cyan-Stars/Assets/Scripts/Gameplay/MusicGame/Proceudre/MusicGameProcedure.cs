using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatLrcParser;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Event;
using CyanStars.Framework.FSM;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Base;
using UnityEngine;
using UnityEngine.SceneManagement;
using UInput = UnityEngine.Input;

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

        //----场景物体与组件--------
        private Scene scene;
        private GameObject sceneRoot;
        private Transform sceneCameraTrans;
        private AudioSource audioSource;

        //----音游相关数据--------
        private MapTimelineData timelineData;
        private string lrcText;
        private AudioClip music;

        //----时间轴相关对象--------
        private Timeline timeline;
        private List<NoteData> linearNoteData = new List<NoteData>();

        //----流程逻辑相关--------
        private BaseInputReceiver inputReceiver;
        private float lastTime = -float.Epsilon;


        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);

            //监听事件
            GameRoot.Event.AddListener(EventConst.MusicGameStartEvent, OnMusicGameStart);
            GameRoot.Event.AddListener(EventConst.MusicGamePauseEvent, OnMusicGamePause);
            GameRoot.Event.AddListener(EventConst.MusicGameResumeEvent, OnMusicGameResume);
            GameRoot.Event.AddListener(EventConst.MusicGameExitEvent, OnMusicGameExit);

            //打开游戏场景
            scene = await GameRoot.Asset.AwaitLoadScene("Assets/BundleRes/Scenes/Dark.unity");

            if (scene != default)
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

        }


        public override void OnExit()
        {
            GameRoot.MainCamera.gameObject.SetActive(true);

            sceneRoot = null;
            sceneCameraTrans = null;
            audioSource = null;

            timelineData = null;
            lrcText = null;
            music = null;

            timeline = null;
            linearNoteData.Clear();

            inputReceiver = null;
            lastTime = -float.Epsilon;

            GameRoot.Event.RemoveListener(EventConst.MusicGameStartEvent, OnMusicGameStart);
            GameRoot.Event.RemoveListener(EventConst.MusicGamePauseEvent, OnMusicGamePause);
            GameRoot.Event.RemoveListener(EventConst.MusicGameResumeEvent, OnMusicGameResume);
            GameRoot.Event.RemoveListener(EventConst.MusicGameExitEvent, OnMusicGameExit);

            CloseMusicGameUI();

            GameRoot.Asset.UnloadScene(scene);
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
            InputMapSO inputMapSo = await GameRoot.Asset.AwaitLoadAsset<InputMapSO>(dataModule.InputMapDataName, sceneRoot);
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
            MapTimelineDataSO timelineDataSo = await GameRoot.Asset.AwaitLoadAsset<MapTimelineDataSO>(mapManifest.TimelineFileName, sceneRoot);
            timelineData = timelineDataSo.Data;
            dataModule.CurTimelineLength = timelineData.Length / 1000f;

            dataModule.CalFullScore(timelineData.NoteTrackData);

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
            GameRoot.UI.OpenUIPanel<MusicGameMainPanel>(null);
            GameRoot.UI.OpenUIPanel<MusicGame3DUIPanel>(null);
        }

        /// <summary>
        /// 关闭音游UI
        /// </summary>
        private void CloseMusicGameUI()
        {
            GameRoot.UI.CloseUIPanel<MusicGameMainPanel>();
            GameRoot.UI.CloseUIPanel<MusicGame3DUIPanel>();
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
            PromptToneTrackData promptToneTrackData = new PromptToneTrackData { ClipDataList = linearNoteData };
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

            GameRoot.Timer.AddUpdateTimer(UpdateTimeline);

            Debug.Log("时间轴创建完毕");
        }

        /// <summary>
        /// 更新时间轴
        /// </summary>
        private void UpdateTimeline(float deltaTime)
        {
            float timelineDeltaTime = audioSource.time - lastTime;
            lastTime = audioSource.time;

            timeline.OnUpdate(timelineDeltaTime);

            //音游流程中 按下ESC打开暂停
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameRoot.UI.OpenUIPanel<MusicGamePausePanel>(null);
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

            GameRoot.Timer.RemoveUpdateTimer(UpdateTimeline);
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
