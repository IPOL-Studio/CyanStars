using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteMovement : MonoBehaviour//音符块的移动代码
{
    private Rigidbody rb;//音符块的刚体组件
    private bool isClicked = false;//音符块是否被击中
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
        if(rb.position.z < Gamesetting.Instance.noteDisappearZ && !isClicked)//如果音符块的z坐标小于消失点z坐标
        {
            Destroy(gameObject,2f);//特效结束后销毁音符块
        }
        rb.velocity = new Vector3(0, 0, -speed);//设置音符块的移动速度
    }
    //碰撞检测
    private void OnTriggerEnter(Collider other)
    {
        if(!isClicked && other.tag == "Key" && Mathf.Abs(transform.position.z - Gamesetting.Instance.noteDisappearZ) <= 15)//如果音符块在判定区域内
        {
            Debug.Log("音符块被击中");
            GameManager.Instance.combo += 1;//Combo数量加1
            isClicked = true;//音符块被击中
            DestoryEffect();
            Destroy(gameObject, 2f);//音符块消失
        }
    }
}
//This code is writed by Ybr.