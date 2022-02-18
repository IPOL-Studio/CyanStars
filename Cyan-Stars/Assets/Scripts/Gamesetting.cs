using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamesetting//游戏的基本设定
{
    public static Gamesetting instance;//游戏的实例
    public static Gamesetting Instance//代理模式
    {
        get
        {
            if(instance == null)//如果游戏的实例为空
            {
                instance = new Gamesetting();//创建游戏的实例
            }
            {
                instance = new Gamesetting();//实例化
            }
            return instance;
        }
    }
    [Header("-----游戏的基本设定-----")]
    [Header("1.音符块消失点z坐标")]
    public float noteDisappearZ = 10f;//音符块消失点z坐标
}
//This code is writed by Ybr.