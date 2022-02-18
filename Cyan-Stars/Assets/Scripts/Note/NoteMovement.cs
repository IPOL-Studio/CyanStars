using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteMovement : MonoBehaviour//音符块的移动代码
{
    private Rigidbody rb;//音符块的刚体组件
    [Header("音符块的移动速度")]
    public float speed = 10f;//音符块的移动速度
    void Start()//初始化
    {
        rb = GetComponent<Rigidbody>();//获取音符块的刚体组件
    }
    public void DestoryEffect()//激活音符块的特效
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
    }
    void Update()//每帧执行
    {
        if(rb.position.z < Gamesetting.Instance.noteDisappearZ)//如果音符块的z坐标小于消失点z坐标
        {
            //TODO:只有点到才有特效
            DestoryEffect();//激活音符块的特效
            Destroy(gameObject,2f);//特效结束后销毁音符块
        }
        rb.velocity = new Vector3(0, 0, -speed);//设置音符块的移动速度
    }
}
//This code is writed by Ybr.