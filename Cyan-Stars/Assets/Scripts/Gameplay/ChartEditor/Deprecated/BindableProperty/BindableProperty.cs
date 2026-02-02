using System;

namespace CyanStars.Gameplay.ChartEditor.BindableProperty
{
    /// <summary>
    /// 用于 MVVM 绑定的可观察的属性包装，在属性引用变化时自动触发 OnValueChanged
    /// </summary>
    /// <remarks>不适用于类实例，因为修改类内数据不会修改引用</remarks>
    public class BindableProperty<T> : IReadonlyBindableProperty<T>
    {
        private T value;
        public event Action<T> OnValueChanged;


        public T Value
        {
            get => value;
            set
            {
                if (Equals(this.value, value))
                {
                    return;
                }

                this.value = value;
                OnValueChanged?.Invoke(this.value);
            }
        }

        public BindableProperty(T initialValue = default)
        {
            value = initialValue;
        }

        /// <summary>
        /// 强制触发一次 OnValueChanged，无论值是否变化，用于 VM 强制刷新 V 中的脏数据
        /// </summary>
        public void ForceNotify()
        {
            OnValueChanged?.Invoke(value);
        }
    }
}
