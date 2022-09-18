using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.Timer
{
    public delegate void TimerCallback();
    public delegate void UpdateTimerCallback(float deltaTime);

    /// <summary>
    /// 定时管理器
    /// </summary>
    public class TimerManager : BaseManager
    {
        /// <inheritdoc />
        public override int Priority { get; }


        private readonly List<UpdateTimerCallback> UpdateTimers = new List<UpdateTimerCallback>();

        private readonly LinkedList<Timer> RunningTimers = new LinkedList<Timer>();
        private readonly List<Timer> WaitRemoveTimers = new List<Timer>();

        public override void OnInit()
        {

        }

        public override void OnUpdate(float deltaTime)
        {
            //处理Update定时器
            if (UpdateTimers.Count > 0)
            {
                for (int i = UpdateTimers.Count - 1; i >= 0; i--)
                {
                    UpdateTimerCallback timer = UpdateTimers[i];
                    timer?.Invoke(deltaTime);
                }
            }

            //处理timer
            if (RunningTimers.Count > 0)
            {
                LinkedListNode<Timer> current = RunningTimers.First;
                while (current != null)
                {
                    if (Time.time >= current.Value.TargetTime)
                    {
                        //已到达目标时间 触发定时回调
                        Timer timer = current.Value;
                        timer.Callback?.Invoke();
                        timer.Counter--;
                        if (timer.Counter != 0)
                        {
                            timer.TargetTime += timer.Interval;
                        }
                        current.Value = timer;

                        //删掉旧的first timer
                        RunningTimers.RemoveFirst();

                        if (current.Value.Counter != 0)
                        {
                            //还有剩余次数
                            //重新插入到timer链表中
                            InternalAddTimer(current);
                        }

                        //处理新的first node
                        current = RunningTimers.First;

                    }
                    else
                    {
                        //first timer未到达目标时间 不进行后续处理了
                        current = null;
                    }
                }

            }

            //删除等待删除的timer
            if (WaitRemoveTimers.Count > 0)
            {
                foreach (Timer waitRemoveTimer in WaitRemoveTimers)
                {
                    RunningTimers.Remove(waitRemoveTimer);
                }
                WaitRemoveTimers.Clear();
            }
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        public void AddTimer(float delay,TimerCallback callback,int count = 1)
        {
            Timer timer = new Timer(Time.time + delay, delay, count, callback);

            if (RunningTimers.Contains(timer))
            {
                Debug.LogError("重复添加了定时器");
                return;
            }


            WaitRemoveTimers.Remove(timer);
            InternalAddTimer(new LinkedListNode<Timer>(timer));
        }

        /// <summary>
        /// 按照目标时间顺序添加定时器
        /// </summary>
        private void InternalAddTimer(LinkedListNode<Timer> timerNode)
        {
            if (RunningTimers.Count == 0)
            {
                RunningTimers.AddFirst(timerNode);
                return;
            }

            LinkedListNode<Timer> current = RunningTimers.First;
            while (current != null)
            {
                if (current.Value.TargetTime > timerNode.Value.TargetTime)
                {
                    //插入到第一个目标时间比当前timer大的timer前
                    RunningTimers.AddBefore(current,timerNode);
                    return;
                }

                current = current.Next;
            }

            //所有timer的目标时间都<=当前timer，就插入到最后
            RunningTimers.AddLast(timerNode);

        }

        /// <summary>
        /// 移除定时器
        /// </summary>
        public void RemoveTimer(TimerCallback callback)
        {
            WaitRemoveTimers.Add(new Timer(default,default,default,callback));
        }


        /// <summary>
        /// 添加Update定时器
        /// </summary>
        public void AddUpdateTimer(UpdateTimerCallback callback)
        {
            if (UpdateTimers.Contains(callback))
            {
                Debug.LogError("重复添加了Update定时器");
                return;
            }
            UpdateTimers.Add(callback);
        }

        /// <summary>
        /// 移除Update定时器
        /// </summary>
        public void RemoveUpdateTimer(UpdateTimerCallback callback)
        {
            UpdateTimers.Remove(callback);
        }
    }
}
