using CyanStars.Framework;

using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符基类
    /// </summary>
    public abstract class BaseNote
    {

        /// <summary>
        /// 此音符所属图层
        /// </summary>
        private NoteLayer layer;

        /// <summary>
        /// 音符数据
        /// </summary>
        protected NoteData Data;

        /// <summary>
        /// 音符位置值
        /// </summary>
        public float Pos => Data.Pos;

        /// <summary>
        /// 判定时间
        /// </summary>
        protected float JudgeTime;

        /// <summary>
        /// 当前逻辑层时间
        /// </summary>
        public float CurLogicTime { get; private set; }

        /// <summary>
        /// 当前逻辑层时间和判定时间的距离
        /// <para> ToFix: https://github.com/IPOL-Studio/CyanStars/issues/231 </para>
        /// </summary>
        public float Distance => JudgeTime - CurLogicTime;

        /// <summary>
        /// 视图层判定时间
        /// </summary>
        private float viewJudgeTime;

        /// <summary>
        /// 当前视图层时间
        /// </summary>
        public float CurViewTime { get; private set; }

        /// <summary>
        /// 当前视图层时间和视图层时间的距离
        /// </summary>
        public float ViewDistance => viewJudgeTime - CurViewTime;

        /// <summary>
        /// 是否创建过视图层物体
        /// </summary>
        private bool createdViewObject = false;

        /// <summary>
        /// 视图层物体
        /// </summary>
        protected IView ViewObject;



        protected MusicGameModule DataModule;

        /// <summary>
        /// 设置数据
        /// </summary>
        public virtual void Init(NoteData data, NoteLayer layer)
        {
            Data = data;
            this.layer = layer;
            JudgeTime = data.JudgeTime / 1000f;
            viewJudgeTime = data.ViewJudgeTime / 1000f;

            DataModule = GameRoot.GetDataModule<MusicGameModule>();

            //考虑性能问题 不再会一开始就创建出所有Note的游戏物体
            //而是需要在viewTimer运行到一个特定时间时再创建
            //viewObject = ViewHelper.CreateViewObject(data);
        }

        /// <summary>
        /// 是否可接收输入
        /// </summary>
        public virtual bool CanReceiveInput()
        {
            return Distance <= EvaluateHelper.CheckInputStartDistance && !EvaluateHelper.IsMiss(Distance);
        }

        /// <summary>
        /// 是否在指定输入范围内
        /// </summary>
        public virtual bool IsInInputRange(float min, float max)
        {
            float left = Data.Pos;
            float right = Data.Pos + NoteData.NoteWidth;

            //3种情况可能重合 1.最左侧在范围内 2.最右侧在范围内 3.中间部分在范围内
            bool result = (left >= min && left <= max)
                          || (right >= min && right <= max)
                          || (left <= min && right >= max);

            return result;
        }

        public virtual void OnUpdate(float curLogicTime,float curViewTime)
        {
            OnBaseUpdate(curLogicTime,curViewTime);
        }

        public virtual void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            OnBaseUpdate(curLogicTime,curViewTime);
        }

        private void OnBaseUpdate(float curLogicTime,float curViewTime)
        {
            CurLogicTime = curLogicTime;
            CurViewTime = curViewTime;

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
    }
}
