using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: 一个统一的单例基类，替换LoggerBase独立的单例实现
public abstract class LoggerBase<T> where T : LoggerBase<T>, new()
{
    private static T instnace;
    public static T Instance => instnace ??= new T();
}
