using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenNote : Note//绿色音符快
{
    private float clickTimer;
    private float timer;
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
            StartCoroutine(StartTiming());
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
        base.Update();
        if(startPoint.position.z < Gamesetting.Instance.noteDisappearZ
        && !(endPoint.position.z < Gamesetting.Instance.noteDisappearZ))
        {
            timer += Time.deltaTime;
        }
        {
            Destroy(gameObject,2f);
            GameManager.Instance.combo = 0;
        }
        if(isTriggered && (!other || !other.gameObject.activeSelf))
        {
            isTriggered = false;
            this.other = null;
            StopAllCoroutines();    //停止所有协程
            Debug.Log(clickTimer/timer);
        }
        if(endPoint.position.z < Gamesetting.Instance.noteDisappearZ)
        {
            Destroy(gameObject,2f);
        }
    }
    IEnumerator StartTiming()
    {
        while(true)
        {
            clickTimer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
//This code is writed by Ybr.