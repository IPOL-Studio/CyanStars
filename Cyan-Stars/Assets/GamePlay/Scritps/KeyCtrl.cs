using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCtrl : MonoBehaviour
{
    public KeyCode key;

    public GameObject body;
    
    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            body.SetActive(true);
        }

        if (Input.GetKeyUp(key))
        {
            body.SetActive(false);
        }
    }
}
