using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 可添加 Key 的时间轴片段基类
    /// </summary>
    public abstract class BaseKeyClip<T> : BaseClip<T>, IKeyableClip
        where T : BaseTrack
    {
        protected List<IKey> Keys;
        protected int ExecutedKeyCount;

        public BaseKeyClip(float startTime, float endTime, T owner) : base(startTime, endTime, owner)
        {
            Keys = new List<IKey>();
        }

        public BaseKeyClip(float startTime, float endTime, T owner, List<IKey> keys) : base(startTime, endTime, owner)
        {
            Keys = keys;
        }

        public virtual void AddKey(IKey key)
        {
            Keys.Add(key);
        }

        public virtual void SortKey()
        {
            Keys.Sort((x, y) => x.Time.CompareTo(y.Time));
        }

        public override void OnUpdate(float currentTime, float previousTime)
        {
            for (int i = ExecutedKeyCount; i < Keys.Count; i++)
            {
                IKey key = Keys[i];

                if (key.Time > currentTime)
                    break;

                key.OnExecute(currentTime);
                ExecutedKeyCount++;
            }
        }
    }
}
