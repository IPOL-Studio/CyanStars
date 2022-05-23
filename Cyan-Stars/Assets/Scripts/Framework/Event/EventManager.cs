using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, HashSet<EventHandler<EventArgs>>> eventHandlerDict =
            new Dictionary<string, HashSet<EventHandler<EventArgs>>>();

        /// <summary>
        /// 等待添加的事件处理方法列表
        /// </summary>
        private readonly List<ValueTuple<string, EventHandler<EventArgs>>> waitAddHanlders =
            new List<ValueTuple<string, EventHandler<EventArgs>>>();

        /// <summary>
        /// 等待删除的事件处理方法列表
        /// </summary>
        private readonly List<ValueTuple<string, EventHandler<EventArgs>>> waitRemoveHanlders =
            new List<ValueTuple<string, EventHandler<EventArgs>>>();


        /// <inheritdoc />
        public override void OnInit()
        {
        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {
            //添加等待添加的事件处理方法
            if (waitAddHanlders.Count != 0)
            {
                for (int i = 0; i < waitAddHanlders.Count; i++)
                {
                    (string eventName, EventHandler<EventArgs> handler) = waitAddHanlders[i];
                    InternalAddListener(eventName, handler);
                }

                waitAddHanlders.Clear();
            }

            //移除等待移除的事件处理方法
            if (waitRemoveHanlders.Count != 0)
            {
                for (int i = 0; i < waitRemoveHanlders.Count; i++)
                {
                    (string eventName, EventHandler<EventArgs> handler) = waitRemoveHanlders[i];
                    InternalRemoveListener(eventName, handler);
                }

                waitRemoveHanlders.Clear();
            }
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        public void AddListener(string eventName, EventHandler<EventArgs> handler)
        {
            //防止在handler回调里添加handler导致报错，统一在onUpdate时再正式添加
            ValueTuple<string, EventHandler<EventArgs>> tuple = (eventName, handler);
            waitAddHanlders.Add(tuple);
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        private void InternalAddListener(string eventName, EventHandler<EventArgs> handler)
        {
            if (!eventHandlerDict.TryGetValue(eventName, out var handlers))
            {
                handlers = new HashSet<EventHandler<EventArgs>>();
                eventHandlerDict.Add(eventName, handlers);
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
            waitRemoveHanlders.Add(tuple);
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        private void InternalRemoveListener(string eventName, EventHandler<EventArgs> handler)
        {
            if (!eventHandlerDict.TryGetValue(eventName, out var handlers))
            {
                return;
            }

            handlers.Remove(handler);
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        public void Dispatch<T>(string eventName, object sender, T eventArgs) where T : EventArgs
        {
            if (!eventHandlerDict.TryGetValue(eventName, out HashSet<EventHandler<EventArgs>> handlers))
            {
                return;
            }

            foreach (EventHandler<EventArgs> handler in handlers)
            {
                handler(sender, eventArgs);
            }
        }
    }
}
