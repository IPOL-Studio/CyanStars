using UnityEngine;

namespace CyanStars.Framework
{
    public abstract class SingletonBase<T> where T : SingletonBase<T>, new()
    {
        private static T instance;
        public static T Instance => instance ??= new T();
    }

    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        public static T Instance { get; private set; }

        protected virtual void OnAwake() {}

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"{typeof(T).Name}已存在单例实例");
                Destroy(this);
                return;
            }
            
            if (this is T instance)
            {
                Instance = instance;
                OnAwake();
            }
            else
            {
                Debug.LogError($"当前实例{GetType().Name}不能分配给单例{typeof(T).Name}");
                Destroy(this);
            }
        }
    }
}
