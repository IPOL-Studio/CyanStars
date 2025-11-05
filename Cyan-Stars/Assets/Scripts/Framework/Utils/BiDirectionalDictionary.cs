#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace CyanStars.Framework.Utils
{
    /// <summary>
    /// 一个双向字典，其中键和值都必须是唯一的。
    /// 提供通过键快速查找值，以及通过值快速查找键的功能。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    public class BiDirectionalDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        // 正向字典：Key -> Value
        private readonly Dictionary<TKey, TValue> Forward = new Dictionary<TKey, TValue>();

        // 反向字典：Value -> Key
        private readonly Dictionary<TValue, TKey> Reverse = new Dictionary<TValue, TKey>();


        /// <summary>
        /// 获取字典中包含的元素数。
        /// </summary>
        public int Count => Forward.Count;

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public BiDirectionalDictionary()
        {
        }

        /// <summary>
        /// 拷贝构造函数，创建一个现有双向字典的浅拷贝。
        /// </summary>
        /// <param name="other">要拷贝的字典。</param>
        public BiDirectionalDictionary(BiDirectionalDictionary<TKey, TValue> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Forward = new Dictionary<TKey, TValue>(other.Forward);
            Reverse = new Dictionary<TValue, TKey>(other.Reverse);
        }


        /// <summary>
        /// 添加一个键值对。如果键或值已存在，则会抛出异常。
        /// </summary>
        /// <param name="key">要添加的键</param>
        /// <param name="value">要添加的值</param>
        /// <exception cref="ArgumentException">已存在一个重复的键或值</exception>
        public void Add(TKey key, TValue value)
        {
            if (Forward.ContainsKey(key))
            {
                throw new ArgumentException($"Duplicate key '{key}' already exists.", nameof(key));
            }

            if (Reverse.ContainsKey(value))
            {
                throw new ArgumentException($"Duplicate value '{value}' already exists.", nameof(value));
            }

            Forward.Add(key, value);
            Reverse.Add(value, key);
        }

        /// <summary>
        /// 根据键获取值。
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">当此方法返回时，如果找到该键，则包含与指定键关联的值；否则，包含 value 参数的类型的默认值。</param>
        /// <returns>如果成功找到值，则为 true；否则为 false。</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return Forward.TryGetValue(key, out value);
        }

        /// <summary>
        /// 根据值获取键。
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="key">当此方法返回时，如果找到该值，则包含与指定值关联的键；否则，包含 key 参数的类型的默认值。</param>
        /// <returns>如果成功找到值，则为 true；否则为 false。</returns>
        public bool TryGetKey(TValue value, out TKey key)
        {
            return Reverse.TryGetValue(value, out key);
        }

        /// <summary>
        /// 根据键移除键值对。
        /// </summary>
        /// <param name="key">要移除的键</param>
        /// <returns>如果成功找到并移除元素，则为 true；否则为 false。</returns>
        public bool RemoveByKey(TKey key)
        {
            if (Forward.TryGetValue(key, out TValue value))
            {
                Forward.Remove(key);
                Reverse.Remove(value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据值移除键值对。
        /// </summary>
        /// <param name="value">要移除的值</param>
        /// <returns>如果成功找到并移除元素，则为 true；否则为 false。</returns>
        public bool RemoveByValue(TValue value)
        {
            if (Reverse.TryGetValue(value, out TKey key))
            {
                Reverse.Remove(value);
                Forward.Remove(key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 清空字典中的所有元素。
        /// </summary>
        public void Clear()
        {
            Forward.Clear();
            Reverse.Clear();
        }

        /// <summary>
        /// 按照正向字典遍历
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Forward.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
