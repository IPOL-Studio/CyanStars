using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour//音符块基类
{
    protected Rigidbody rb;//音符块的刚体组件
    protected bool isClicked = false;//音符块是否被击中
    [Header("音符块的移动速度")]
    public float speed = 10f;//音符块的移动速度
    [Header("音符块的分数")]
    public int score = 1;//音符块的分数
    void Start()//初始化
    {
        rb = GetComponent<Rigidbody>();//获取音符块的刚体组件
    }
    public void DestoryEffect()//激活音符块的特效
    {
        //TODO:修改音符块的特效
        transform.GetChild(0).gameObject.SetActive(true);
    }
    protected void Update()//每帧执行
    {
        if(transform.position.z < Gamesetting.Instance.noteDisappearZ && !isClicked)//如果音符块的z坐标小于消失点z坐标
        {
            Destroy(gameObject,2f);//特效结束后销毁音符块
            GameManager.Instance.combo = 0;//清除连击数
        }
        rb.velocity = new Vector3(0, 0, -speed);//设置音符块的移动速度
    }
}
//This code is writed by Ybr.