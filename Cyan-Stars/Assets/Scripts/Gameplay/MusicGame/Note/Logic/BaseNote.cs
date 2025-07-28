using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public abstract class BaseNote
    {
        /// <summary>
        /// 是否创建过视图层物体
        /// </summary>
        private bool createdViewObject;

        /// <summary>
        /// 判定时间（逻辑层时间）（s）
        /// </summary>
        protected float JudgeTime;

        /// <summary>
        /// 拥有此音符的片段
        /// </summary>
        public NoteClip NoteClip;

        /// <summary>
        /// 音符数据
        /// </summary>
        protected BaseChartNoteData NoteData;

        /// <summary>
        /// 引用的变速组实例
        /// </summary>
        protected SpeedGroup SpeedGroup;

        /// <summary>
        /// 视图层物体
        /// </summary>
        protected IView ViewObject;

        /// <summary>
        /// 当前逻辑层时间（s）
        /// </summary>
        public float CurLogicTime { get; private set; }

        /// <summary>
        /// 当前逻辑层时间和判定时间的距离（s）
        /// </summary>
        /// <remarks>提前时距离为负数</remarks>
        public float LogicTimeDistance => CurLogicTime - JudgeTime;

        /// <summary>
        /// 当前视图层时间与视图层时间的差（音符到判定线的距离）
        /// </summary>
        /// <remarks>提前时距离为负数</remarks>
        public float CurViewDistance { get; private set; }


        /// <summary>
        /// 初始化数据
        /// </summary>
        public virtual void Init(BaseChartNoteData data, ChartData chartData, NoteClip clip)
        {
            NoteClip = clip;
            NoteData = data;
            SpeedGroup = new SpeedGroup(chartData.SpeedGroupDatas[data.SpeedGroupIndex]);

            // 根据 beat 计算 JudgeTime
            // 注意 Offset 是作为空白时间直接加（或减）在 MisicTrack/MusicClip 中，与 Note 判定时间无关
            JudgeTime = chartData.BpmGroups.CalculateTime(data.JudgeBeat) / 1000f;
        }

        public virtual void OnUpdate(float curLogicTime)
        {
            OnBaseUpdate(curLogicTime);
        }

        public virtual void OnUpdateInAutoMode(float curLogicTime)
        {
            OnBaseUpdate(curLogicTime);
        }

        private void OnBaseUpdate(float curLogicTime)
        {
            CurLogicTime = curLogicTime;
            CurViewDistance = SpeedGroup.GetDistance(LogicTimeDistance * 1000f);
            TryCreateViewObject();

            ViewObject?.OnUpdate(CurViewDistance);
        }

        /// <summary>
        /// 尝试创建视图层物体
        /// </summary>
        private async void TryCreateViewObject()
        {
            if (!createdViewObject && CurViewDistance <= ViewHelper.ViewObjectCreateDistance)
            {
                //到创建视图层物体的时间点了
                createdViewObject = true;

                ViewObject = await ViewHelper.CreateViewObject(NoteData, this);
            }
        }

        /// <summary>
        /// 此音符有对应输入时
        /// </summary>
        public virtual void OnInput(InputType inputType)
        {
            //Debug.Log($"音符接收输入:输入类型{inputType},倒计时器:{timer},数据{data}");
        }

        /// <summary>
        /// 销毁自身
        /// </summary>
        protected void DestroySelf(bool autoMove = true)
        {
            NoteClip.Notes.Remove(this);
            ViewObject.DestroySelf(autoMove);
            ViewObject = null;
        }

        /// <summary>
        /// 是否可接收输入
        /// </summary>
        public virtual bool CanReceiveInput()
        {
            return LogicTimeDistance >= EvaluateHelper.CheckInputStartDistance &&
                   !EvaluateHelper.IsMiss(LogicTimeDistance);
        }

        /// <summary>
        /// 是否在指定输入范围内
        /// </summary>
        public abstract bool IsInInputRange(float min, float max);
    }
}
