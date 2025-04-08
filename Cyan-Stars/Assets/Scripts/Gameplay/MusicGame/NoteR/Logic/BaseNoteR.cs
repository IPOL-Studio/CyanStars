using System.Linq;

namespace CyanStars.Gameplay.MusicGame
{
    public class BaseNoteR
    {
        /// <summary>
        /// 音符数据
        /// </summary>
        protected BaseChartNoteData NoteData;

        /// <summary>
        /// 引用的变速组实例
        /// </summary>
        protected SpeedGroup SpeedGroup;

        /// <summary>
        /// 判定时间（逻辑层时间）（s）
        /// </summary>
        protected float JudgeTime;

        /// <summary>
        /// 当前逻辑层时间
        /// </summary>
        public float CurLogicTime { get; private set; }

        /// <summary>
        /// 当前逻辑层时间和判定时间的距离（s）
        /// </summary>
        public float LogicTimeDistance => CurLogicTime - JudgeTime;

        /// <summary>
        /// 当前视图层时间与视图层时间的差（音符到判定线的距离）
        /// </summary>
        /// <remarks>提前时距离为负数</remarks>
        public float ViewDistance { get; private set; }

        /// <summary>
        /// 是否创建过视图层物体
        /// </summary>
        private bool createdViewObject = false;

        /// <summary>
        /// 视图层物体
        /// </summary>
        protected IView ViewObject;


        /// <summary>
        /// 初始化数据
        /// </summary>
        public virtual void Init(BaseChartNoteData data, ChartData chartData)
        {
            NoteData = data;

            // 根据 beat 计算 JudgeTime
            // 注意 Offset 是作为空白时间直接加（或减）在 MisicTrack/MusicClip 中，与 Note 判定时间无关
            JudgeTime = 0;
            if (chartData.BpmGroups != null && chartData.BpmGroups.Count > 0)
            {
                float judgeBeat = data.JudgeBeat.ToFloat();
                bool foundGroupFlag = false;
                for (int i = 0; i < chartData.BpmGroups.Count - 1; i++)
                {
                    BpmGroup current = chartData.BpmGroups[i];
                    BpmGroup next = chartData.BpmGroups[i + 1];
                    if (judgeBeat >= next.StartBeat.ToFloat())
                    {
                        // 累加当前组的完整时长
                        JudgeTime += (next.StartBeat.ToFloat() - current.StartBeat.ToFloat())
                                     * (60 / current.Bpm) * 1000;
                    }
                    else
                    {
                        // 计算当前组的部分时间
                        JudgeTime += (judgeBeat - current.StartBeat.ToFloat()) * (60 / current.Bpm) * 1000;
                        foundGroupFlag = true;
                        break;
                    }
                }

                // 处理最后一个BPM组的情况
                if (!foundGroupFlag)
                {
                    BpmGroup lastGroup = chartData.BpmGroups.Last();
                    JudgeTime += (judgeBeat - lastGroup.StartBeat.ToFloat()) * (60 / lastGroup.Bpm) * 1000;
                }
            }
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
            ViewDistance = SpeedGroup.CalculateDistance(LogicTimeDistance * 1000f);
            TryCreateViewObject();

            ViewObject?.OnUpdate(ViewDistance);
        }

        /// <summary>
        /// 尝试创建视图层物体
        /// </summary>
        private async void TryCreateViewObject()
        {
            if (!createdViewObject && ViewDistance <= ViewHelper.ViewObjectCreateDistance)
            {
                //到创建视图层物体的时间点了
                createdViewObject = true;

                ViewObject = await ViewHelper.CreateViewObject(Data, this);
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
            layer.RemoveNote(this);
            ViewObject.DestroySelf(autoMove);
            ViewObject = null;
        }

        /// <summary>
        /// 是否可接收输入
        /// </summary>
        public virtual bool CanReceiveInput()
        {
            return LogicTimeDistance <= EvaluateHelper.CheckInputStartDistance && !EvaluateHelper.IsMiss(Distance);
        }
    }
}
