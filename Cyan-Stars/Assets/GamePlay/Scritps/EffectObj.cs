using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObj : MonoBehaviour
{
    public float destroyTime = -1;

    void Start()
    {
        if(destroyTime > 0)
        {
            Destroy(gameObject, destroyTime);
        }
    }
}
