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


        private List<UpdateTimerCallback> updateTimers = new List<UpdateTimerCallback>();

        private LinkedList<Timer> runningTimers = new LinkedList<Timer>();
        private List<Timer> waitRemoveTimers = new List<Timer>();

        public override void OnInit()
        {

        }

        public override void OnUpdate(float deltaTime)
        {
            //处理Update定时器
            if (updateTimers.Count > 0)
            {
                for (int i = updateTimers.Count - 1; i >= 0; i--)
                {
                    UpdateTimerCallback timer = updateTimers[i];
                    timer?.Invoke(deltaTime);
                }
            }

            //处理timer
            if (runningTimers.Count > 0)
            {
                LinkedListNode<Timer> current = runningTimers.First;
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
                        runningTimers.RemoveFirst();

                        if (current.Value.Counter != 0)
                        {
                            //还有剩余次数
                            //重新插入到timer链表中
                            InternalAddTimer(current);
                        }

                        //处理新的first node
                        current = runningTimers.First;

                    }
                    else
                    {
                        //first timer未到达目标时间 不进行后续处理了
                        current = null;
                    }
                }

            }

            //删除等待删除的timer
            if (waitRemoveTimers.Count > 0)
            {
                foreach (Timer waitRemoveTimer in waitRemoveTimers)
                {
                    runningTimers.Remove(waitRemoveTimer);
                }
                waitRemoveTimers.Clear();
            }
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        public void AddTimer(float delay,TimerCallback callback,int count = 1)
        {
            Timer timer = new Timer(Time.time + delay, delay, count, callback);

            if (runningTimers.Contains(timer))
            {
                Debug.LogError("重复添加了定时器");
                return;
            }


            waitRemoveTimers.Remove(timer);
            InternalAddTimer(new LinkedListNode<Timer>(timer));
        }

        /// <summary>
        /// 按照目标时间顺序添加定时器
        /// </summary>
        private void InternalAddTimer(LinkedListNode<Timer> timerNode)
        {
            if (runningTimers.Count == 0)
            {
                runningTimers.AddFirst(timerNode);
                return;
            }

            LinkedListNode<Timer> current = runningTimers.First;
            while (current != null)
            {
                if (current.Value.TargetTime > timerNode.Value.TargetTime)
                {
                    //插入到第一个目标时间比当前timer大的timer前
                    runningTimers.AddBefore(current,timerNode);
                    return;
                }

                current = current.Next;
            }

            //所有timer的目标时间都<=当前timer，就插入到最后
            runningTimers.AddLast(timerNode);

        }

        /// <summary>
        /// 移除定时器
        /// </summary>
        public void RemoveTimer(TimerCallback callback)
        {
            waitRemoveTimers.Add(new Timer(default,default,default,callback));
        }


        /// <summary>
        /// 添加Update定时器
        /// </summary>
        public void AddUpdateTimer(UpdateTimerCallback callback)
        {
            if (updateTimers.Contains(callback))
            {
                Debug.LogError("重复添加了Update定时器");
                return;
            }
            updateTimers.Add(callback);
        }

        /// <summary>
        /// 移除Update定时器
        /// </summary>
        public void RemoveUpdateTimer(UpdateTimerCallback callback)
        {
            updateTimers.Remove(callback);
        }
    }
}
