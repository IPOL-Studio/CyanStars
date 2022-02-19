using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenNote : Note//绿色音符快
{
    public float clickTimer;
    public float timer;
    public bool isClick;
    [Header("绿色音符块的开始点")]
    public Transform startPoint;
    [Header("绿色音符块的结束点")]
    public Transform endPoint;
    private Collider other;
    private bool isTriggered = false;
    void OnTriggerEnter(Collider other)
    {
        if(!isClicked && other.tag == "Key" && 
        Mathf.Abs(startPoint.position.z - Gamesetting.Instance.noteDisappearZ) <= 10 &&
         !(startPoint.position.z < Gamesetting.Instance.noteDisappearZ))
        {
            clickTimer = 0;
            isClicked = true;
            this.other = other;
            isTriggered = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Key")
        {
            isTriggered = false;
            this.other = null;
            StopAllCoroutines();    //停止所有协程
        }
    }
    new void Update()
    {
        Debug.Log(clickTimer + " " + timer);
        base.Update();
        if(startPoint.position.z < Gamesetting.Instance.noteDisappearZ
        && !(endPoint.position.z < Gamesetting.Instance.noteDisappearZ))
        {
            timer += Time.deltaTime;
            if(isTriggered)
            {
                clickTimer += Time.deltaTime;
            }
        }
        if(isTriggered)DestoryEffect();
        else transform.GetChild(0).gameObject.SetActive(false);
        if(isTriggered && (!other || !other.gameObject.activeSelf))
        {
            isTriggered = false;
            this.other = null;
            if(clickTimer/timer < 0.5f)
            {
                GameManager.Instance.combo = 0;
            }
            else if(clickTimer/timer < 0.9f)
            {
                GameManager.Instance.combo++;
                GameManager.Instance.score += score;
            }
            else
            {
                GameManager.Instance.combo++;
                GameManager.Instance.score += score * 2;
            }
            GameManager.Instance.combo += 1;
        }
        if(endPoint.position.z < Gamesetting.Instance.noteDisappearZ)
        {
            Destroy(gameObject,2f);
        }
    }
}
//This code is writed by Ybr.