using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueNote : Note//蓝色音符块
{
    //碰撞检测
    private void OnTriggerEnter(Collider other)
    {
        if(!isClicked && other.tag == "Key" && 
        Mathf.Abs(transform.position.z - Gamesetting.Instance.noteDisappearZ) <= 10 &&
         !(transform.position.z < Gamesetting.Instance.noteDisappearZ))//如果音符块在判定区域内
        {
            GameManager.Instance.combo += 1;//Combo数量加1
            GameManager.Instance.score += score;//分数加上音符块的分数
            isClicked = true;//音符块被击中
            DestoryEffect();
            Destroy(gameObject, 2f);//音符块消失
        }
    }
}
//This code is writed by Ybr.