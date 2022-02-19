using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;//游戏的实例
    public static GameManager Instance//代理模式
    {
        get
        {
            if(instance == null)
            {
                instance = Transform.FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }
    [Header("-----游戏数据-----")]
    [Header("1.Combo数量")]
    public int combo = 0;//Combo数量
    [Header("2.分数")]
    public int score = 0;//分数
}
//This code is writed by Ybr.