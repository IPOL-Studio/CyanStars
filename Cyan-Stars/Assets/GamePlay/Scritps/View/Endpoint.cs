using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endpoint : MonoBehaviour
{  
    public static Endpoint Instance;
    public GameObject leftObj;
    public GameObject rightObj;
    void Start()
    {
        Instance = this;
    }
    public float GetLeftPos()
    {
        return leftObj.transform.position.x;
    }
    public float GetRightPos()
    {
        return rightObj.transform.position.x;
    }
    public float GetLenth()
    {
        return rightObj.transform.position.x - leftObj.transform.position.x;
    }
    public float GetPos(float ratio)
    {
        return leftObj.transform.position.x + ratio * (rightObj.transform.position.x - leftObj.transform.position.x);
    }
}
