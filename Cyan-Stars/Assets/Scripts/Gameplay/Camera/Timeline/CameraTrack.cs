using System.Collections;
using System.Collections.Generic;
using CatTimeline;
using UnityEngine;

/// <summary>
/// 相机轨道
/// </summary>
public class CameraTrack : BaseTrack
{
    public Vector3 DefaultCameraPos;
    public Transform CameraTrans;

    public Vector3 oldRot;
    
    /// <summary>
    /// 创建相机轨道片段
    /// </summary>
    public static BaseClip<CameraTrack> CreateClip(CameraTrack track, int clipIndex, object userdata)
    {
        List<CameraControllerSo.KeyFrame> keyFrames = (List<CameraControllerSo.KeyFrame>)userdata;
        CameraControllerSo.KeyFrame keyFrame = keyFrames[clipIndex];

        float startTime = 0;
        if (clipIndex > 0)
        {
            startTime = keyFrames[clipIndex - 1].time;
        }
            
        CameraClip clip = new CameraClip(startTime / 1000f, keyFrame.time / 1000f, track, keyFrame.position, keyFrame.rotation,keyFrame.smoothType);

        return clip;
    }
}
