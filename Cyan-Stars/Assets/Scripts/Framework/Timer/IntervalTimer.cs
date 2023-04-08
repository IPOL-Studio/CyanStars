using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CyanStars.Framework.Timer
{
    public delegate void TimerCallback(object userdata);

    public sealed class IntervalTimer : ITimer
    {
        private struct Timer : IEquatable<Timer>
        {
            /// <summary>
            /// 目标时间
            /// </summary>
            public float TargetTime;

            /// <summary>
            /// 间隔
            /// </summary>
            public readonly float Interval;

            /// <summary>
            /// 剩余次数
            /// </summary>
            public int Counter;

            /// <summary>
            /// 定时回调
            /// </summary>
            public readonly TimerCallback Callback;

            /// <summary>
            /// 用户自定义数据
            /// </summary>
            public readonly object Userdata;

            public readonly CancellationToken CancellationToken;

            public Timer(float targetTime, float interval, int counter, TimerCallback callback, object userdata = null, CancellationToken cancellationToken = default)
            {
                TargetTime = targetTime;
                Interval = interval;
                Counter = counter;
                Callback = callback;
                Userdata = userdata;
                CancellationToken = cancellationToken;
            }

            public bool Equals(Timer other)
            {
                return Callback == other.Callback;
            }

            public override bool Equals(object obj)
            {
                return obj is Timer other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Callback != null ? Callback.GetHashCode() : 0;
            }
        }


        private LinkedList<Timer> runningTimers = new LinkedList<Timer>();
        private List<Timer> waitRemoveTimers = new List<Timer>();

        public void OnUpdate(float deltaTime)
        {
            //处理timer
            if (runningTimers.Count > 0)
            {
                LinkedListNode<Timer> current = runningTimers.First;
                while (current != null)
                {
                    Timer timer = current.Value;

                    if (timer.CancellationToken.IsCancellationRequested)
                    {
                        waitRemoveTimers.Add(timer);
                    }

                    if (Time.time >= timer.TargetTime)
                    {
                        //已到达目标时间 触发定时回调
                        timer.Callback?.Invoke(timer.Userdata);
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
                            InternalAdd(current);
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
        public void Add(float delay, TimerCallback callback, object userdata = null, int count = 1, CancellationToken cancellationToken = default)
        {
            Timer timer = new Timer(Time.time + delay, delay, count, callback, userdata, cancellationToken);
            if (runningTimers.Contains(timer))
            {
                Debug.LogError("重复添加了定时器");
                return;
            }

            waitRemoveTimers.Remove(timer);
            InternalAdd(new LinkedListNode<Timer>(timer));
        }

        /// <summary>
        /// 按照目标时间顺序添加定时器
        /// </summary>
        private void InternalAdd(LinkedListNode<Timer> timerNode)
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
                    runningTimers.AddBefore(current, timerNode);
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
        public void Remove(TimerCallback callback)
        {
            waitRemoveTimers.Add(new Timer(default, default, default, default, callback));
        }
    }
}
