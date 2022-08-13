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

        //----场景物体与组件--------
        private GameObject sceneRoot;
        private Transform sceneCameraTrans;
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
        private List<NoteData> linearNoteData = new List<NoteData>();

        //----流程逻辑相关--------
        private HashSet<KeyCode> pressedKeySet = new HashSet<KeyCode>();
        private float lastTime = -float.Epsilon;
        private bool isStartGame = false;


        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);

            //监听事件
            GameRoot.Event.AddListener(EventConst.MusicGameStartEvent, StartMusicGame);
            GameRoot.Event.AddListener(EventConst.MusicGamePauseEvent, PauseMusicGame);
            GameRoot.Event.AddListener(EventConst.MusicGameResumeEvent, ResumeMusicGame);
            GameRoot.Event.AddListener(EventConst.MusicGameExitEvent, ExitMusicGame);

            GameRoot.Event.AddListener(InputEventArgs.EventName,OnInput);

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

                float timelineDeltaTime = audioSource.time - lastTime;
                lastTime = audioSource.time;

                timeline.OnUpdate(timelineDeltaTime);
            }

            //音游流程中 按下ESC打开暂停
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameRoot.UI.OpenUIPanel<MusicGamePausePanel>(null);
            }
        }

        public override void OnExit()
        {
            GameRoot.MainCamera.gameObject.SetActive(true);

            sceneCameraTrans = null;
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
            linearNoteData.Clear();

            pressedKeySet.Clear();
            lastTime = -float.Epsilon;
            isStartGame = false;

            GameRoot.Event.RemoveListener(EventConst.MusicGameStartEvent, StartMusicGame);
            GameRoot.Event.RemoveListener(EventConst.MusicGamePauseEvent, PauseMusicGame);
            GameRoot.Event.RemoveListener(EventConst.MusicGameResumeEvent, ResumeMusicGame);
            GameRoot.Event.RemoveListener(EventConst.MusicGameExitEvent, ExitMusicGame);
            GameRoot.Event.RemoveListener(InputEventArgs.EventName,OnInput);

            CloseMusicGameUI();

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
        /// 暂停音游
        /// </summary>
        private void PauseMusicGame(object sender, EventArgs e)
        {
            isStartGame = false;
            audioSource.Pause();
        }

        /// <summary>
        /// 恢复音游
        /// </summary>
        private void ResumeMusicGame(object sender, EventArgs e)
        {
            isStartGame = true;
            audioSource.UnPause();
        }

        /// <summary>
        /// 退出音游
        /// </summary>
        private void ExitMusicGame(object sender, EventArgs e)
        {
            dataModule.ResetPlayingData();
            GameRoot.ChangeProcedure<MainHomeProcedure>();
        }

        /// <summary>
        /// 输入监听
        /// </summary>
        private void OnInput(object sender, EventArgs e)
        {
            if (!isStartGame || timeline == null)
            {
                return;
            }

            InputEventArgs args = (InputEventArgs)e;
            ReceiveInput(args.Type,args.Item);
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

            if (Application.platform == RuntimePlatform.Android)
            {
                sceneRoot.transform.Find("TouchInputReceiverGenerator")
                    .GetComponent<TouchInputReceiverGenerator>()
                    .SetInputMapData(inputMapSo.InputMapData);
            }

            //谱面清单
            mapManifest = dataModule.GetMap(dataModule.MapIndex);

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

            Timeline timeline = new Timeline(data.Length/ 1000f);
            timeline.OnStop += () =>
            {
                this.timeline = null;
                isStartGame = false;
                lastTime = -float.Epsilon;
                GameRoot.Event.Dispatch(EventConst.MusicGameEndEvent,this,EmptyEventArgs.Create());

                Debug.Log("音游结束");
            };

            //添加音符轨道
            noteTrack = timeline.AddTrack(data.NoteTrackData, NoteTrack.CreateClipFunc);

            if (!string.IsNullOrEmpty(lrcText))
            {
                //添加歌词轨道
                Lyric lrc = LrcParser.Parse(lrcText);
                LrcTrackData lrcTrackData = new LrcTrackData { ClipDataList = lrc.TimeTagList };

                LrcTrack lrcTrack = timeline.AddTrack(lrcTrackData, LrcTrack.CreateClipFunc);
                lrcTrack.TxtLrc = GameRoot.UI.GetUIPanel<MusicGameMainPanel>().TxtLrc;
            }

            //添加相机轨道
            CameraTrack cameraTrack = timeline.AddTrack(data.CameraTrackData, CameraTrack.CreateClipFunc);
            cameraTrack.DefaultCameraPos = data.CameraTrackData.DefaultPosition;
            cameraTrack.OldRot = data.CameraTrackData.DefaultRotation;
            cameraTrack.CameraTrans = sceneCameraTrans.transform;


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
            EffectTrack effectTrack = timeline.AddTrack(data.EffectTrackData, EffectTrack.CreateClipFunc);
            effectTrack.EffectNames = dataModule.EffectNames;
            effectTrack.EffectParent = ViewHelper.EffectRoot;
            effectTrack.ImgFrame = GameRoot.UI.GetUIPanel<MusicGameMainPanel>().ImgFrame;

            this.timeline = timeline;
            dataModule.RunningTimeline = timeline;
            Debug.Log("时间轴创建完毕");
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

        /// <summary>
        /// 检查键盘输入
        /// </summary>
        private void CheckKeyboardInput()
        {
            for (int i = 0; i < inputMapData.Items.Count; i++)
            {
                InputMapData.Item item = inputMapData.Items[i];
                if (UInput.GetKeyDown(item.Key))
                {
                    pressedKeySet.Add(item.Key);
                    ReceiveInput(InputType.Down, item);
                    continue;
                }

                if (UInput.GetKey(item.Key))
                {
                    ReceiveInput(InputType.Press, item);
                    continue;
                }

                if (pressedKeySet.Remove(item.Key))
                {
                    ReceiveInput(InputType.Up, item);
                }
            }
        }

        /// <summary>
        /// 接收输入
        /// </summary>
        private void ReceiveInput(InputType inputType, InputMapData.Item item)
        {
            if (dataModule.IsAutoMode)
            {
                return;
            }

            if (noteTrack == null)
            {
                Debug.LogError("noteTrack为null");
                return;
            }

            switch (inputType)
            {
                case InputType.Down:
                    keyViewController.KeyDown(item);
                    break;

                case InputType.Up:
                    keyViewController.KeyUp(item);
                    break;


            }

            noteTrack.OnInput(inputType, item);
        }
    }
}
