/// <summary>
/// 一个表示可选更新值的结构体。
/// 它能区分“未提供值”和“提供的值是 null”。
/// </summary>
/// <typeparam name="T">值的类型</typeparam>
public struct UpdateValue<T>
{
    public readonly T Value;
    public readonly bool NeedUpdate;

    public UpdateValue(T value)
    {
        Value = value;
        NeedUpdate = true;
    }

    public static implicit operator UpdateValue<T>(T value)
    {
        return new UpdateValue<T>(value);
    }
}
