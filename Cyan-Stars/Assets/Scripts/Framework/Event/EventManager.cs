using System;
using System.Collections.Generic;
using CatAsset.Runtime;
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
        /// 临时事件容器的池
        /// </summary>
        private readonly Queue<HashSet<EventHandler<EventArgs>>> TempHandlersPool =
            new Queue<HashSet<EventHandler<EventArgs>>>();


        /// <inheritdoc />
        public override void OnInit()
        {
        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        public void AddListener(string eventName, EventHandler<EventArgs> handler)
        {
            if (!EventHandlerDict.TryGetValue(eventName, out HashSet<EventHandler<EventArgs>> handlers))
            {
                handlers = new HashSet<EventHandler<EventArgs>>();
                EventHandlerDict.Add(eventName, handlers);
            }

            if (handlers.Contains(handler))
            {
                Debug.LogError($"禁止重复添加事件处理方法：{eventName}");
                return;
            }

            handlers.Add(handler);
        }


        /// <summary>
        /// 移除事件监听
        /// </summary>
        public void RemoveListener(string eventName, EventHandler<EventArgs> handler)
        {
            if (!EventHandlerDict.TryGetValue(eventName, out HashSet<EventHandler<EventArgs>> handlers))
            {
                return;
            }

            handlers.Remove(handler);
        }


        /// <summary>
        /// 派发事件
        /// </summary>
        public void Dispatch<T>(string eventName, object sender, T eventArgs) where T : EventArgs, IReference
        {
            if (!EventHandlerDict.TryGetValue(eventName, out HashSet<EventHandler<EventArgs>> handlers))
            {
                return;
            }

            //使用临时的事件容器进行事件派发，防止在事件派发过程中添加或移除监听导致报错
            //因此添加或移除监听只有在下一次派发事件时才会发生效果
            HashSet<EventHandler<EventArgs>> tempHandlers = GetTempHandlers();
            foreach (EventHandler<EventArgs> handler in handlers)
            {
                tempHandlers.Add(handler);
            }

            foreach (EventHandler<EventArgs> handler in tempHandlers)
            {
                handler?.Invoke(sender, eventArgs);
            }

            ReleaseTempHandlers(tempHandlers);
            ReferencePool.Release(eventArgs);
        }

        /// <summary>
        /// 获取临时事件容器
        /// </summary>
        private HashSet<EventHandler<EventArgs>> GetTempHandlers()
        {
            if (TempHandlersPool.Count == 0)
            {
                TempHandlersPool.Enqueue(new HashSet<EventHandler<EventArgs>>());
            }

            return TempHandlersPool.Dequeue();
        }

        /// <summary>
        /// 归还临时事件容器
        /// </summary>
        private void ReleaseTempHandlers(HashSet<EventHandler<EventArgs>> tempHandlers)
        {
            tempHandlers.Clear();
            TempHandlersPool.Enqueue(tempHandlers);
        }
    }
}
