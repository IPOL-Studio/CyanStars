using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteClickEffect : MonoBehaviour
{
    public bool willDestroy;
    public float destroyTime;

    private Transform trans;
    
    void Start()
    {
        trans = transform;
        //Debug.Log(transform.position);
        if (willDestroy) Destroy(gameObject, destroyTime);
    }
    void Update()
    {
        var pos = trans.position;
        pos.z = 0;
        trans.position = pos;
    }
}
