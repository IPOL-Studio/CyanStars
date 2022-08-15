using System;
using System.Collections.Generic;
using CyanStars.Framework.Pool;
using UnityEngine;

namespace CyanStars.Framework.Event
{
    /// <summary>
    /// 事件管理器
    /// </summary>
    public class EventManager : BaseManager
    {
        /// <inheritdoc />
        public override int Priority { get; }

        /// <summary>
        /// 事件名->事件处理方法集合
        /// </summary>
        private readonly Dictionary<string, HashSet<EventHandler<EventArgs>>> EventHandlerDict =
            new Dictionary<string, HashSet<EventHandler<EventArgs>>>();

        /// <summary>
        /// 等待添加的事件处理方法列表
        /// </summary>
        private readonly List<ValueTuple<string, EventHandler<EventArgs>>> WaitAddHandlers =
            new List<ValueTuple<string, EventHandler<EventArgs>>>();

        /// <summary>
        /// 等待删除的事件处理方法列表
        /// </summary>
        private readonly List<ValueTuple<string, EventHandler<EventArgs>>> WaitRemoveHandlers =
            new List<ValueTuple<string, EventHandler<EventArgs>>>();


        /// <inheritdoc />
        public override void OnInit()
        {
        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {
            //添加等待添加的事件处理方法
            if (WaitAddHandlers.Count != 0)
            {
                for (int i = 0; i < WaitAddHandlers.Count; i++)
                {
                    (string eventName, EventHandler<EventArgs> handler) = WaitAddHandlers[i];
                    InternalAddListener(eventName, handler);
                }

                WaitAddHandlers.Clear();
            }

            //移除等待移除的事件处理方法
            if (WaitRemoveHandlers.Count != 0)
            {
                for (int i = 0; i < WaitRemoveHandlers.Count; i++)
                {
                    (string eventName, EventHandler<EventArgs> handler) = WaitRemoveHandlers[i];
                    InternalRemoveListener(eventName, handler);
                }

                WaitRemoveHandlers.Clear();
            }
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        public void AddListener(string eventName, EventHandler<EventArgs> handler)
        {
            //防止在handler回调里添加handler导致报错，统一在onUpdate时再正式添加
            ValueTuple<string, EventHandler<EventArgs>> tuple = (eventName, handler);
            WaitAddHandlers.Add(tuple);
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        private void InternalAddListener(string eventName, EventHandler<EventArgs> handler)
        {
            if (!EventHandlerDict.TryGetValue(eventName, out var handlers))
            {
                handlers = new HashSet<EventHandler<EventArgs>>();
                EventHandlerDict.Add(eventName, handlers);
            }

            if (handlers.Contains(handler))
            {
                Debug.LogError($"禁止重复添加事件处理函数：{eventName}");
                return;
            }

            handlers.Add(handler);
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public void RemoveListener(string eventName, EventHandler<EventArgs> handler)
        {
            //防止在handler回调里移除handler导致报错，统一在onUpdate时再正式移除
            ValueTuple<string, EventHandler<EventArgs>> tuple = (eventName, handler);
            WaitRemoveHandlers.Add(tuple);
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        private void InternalRemoveListener(string eventName, EventHandler<EventArgs> handler)
        {
            if (!EventHandlerDict.TryGetValue(eventName, out var handlers))
            {
                return;
            }

            handlers.Remove(handler);
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        public void Dispatch<T>(string eventName, object sender, T eventArgs) where T : EventArgs,IReference
        {
            if (!EventHandlerDict.TryGetValue(eventName, out HashSet<EventHandler<EventArgs>> handlers))
            {
                return;
            }

            foreach (EventHandler<EventArgs> handler in handlers)
            {
                handler(sender, eventArgs);
            }
            ReferencePool.Release(eventArgs);
        }
    }
}
