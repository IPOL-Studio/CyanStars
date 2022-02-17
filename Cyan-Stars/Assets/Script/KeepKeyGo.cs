using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这个脚本用于实现key的向前滑入特效

public class KeepKeyGo : MonoBehaviour
{
    public float keySpeed = 30f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.z < 42.5f)
        {
            if (transform.position.z + keySpeed * Time.deltaTime < 42.5f)
            {
                transform.Translate(0, 0, keySpeed * Time.deltaTime);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, 42.5f);
            }
        }
    }
}
