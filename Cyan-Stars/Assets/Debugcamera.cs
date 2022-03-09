using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugcamera : MonoBehaviour
{
    // Start is called before the first frame update
    CameraController cameraController;
    Vector3 pos , rot,pos1;
    float dtime,dtime2;

    void Start()
    {
        cameraController = GetComponent<CameraController>();
        pos.x = 0f;
        pos.y = pos.z = 0f;
        rot.x = 0f;
        rot.y = rot.z = 0f;
        pos1.x = 90f;
        pos1.y = pos1.z = 0f;
        dtime = 3000f;
        dtime2 = 6000f;
        print("1");
        cameraController.MoveCamera(pos, rot, dtime2, SmoothFuncationType.Linear);
        cameraController.MoveCamera(pos, pos1, dtime, SmoothFuncationType.Linear);
    }
    private void Update()
    {
        //cameraController.MoveCamera(pos, rot, dtime, SmoothFuncationType.Linear);
       // cameraController.MoveCamera(pos, pos1, dtime, SmoothFuncationType.Linear);
    }
}