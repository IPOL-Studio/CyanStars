using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteClickEffect : MonoBehaviour
{
    public bool willDestroy;
    public float destroyTime;
    void Start()
    {
        //Debug.Log(transform.position);
        if(willDestroy)Destroy(gameObject,destroyTime);
    }
    void Update()
    {
        transform.position = new Vector3(transform.position.x,transform.position.y,10);
    }
}
