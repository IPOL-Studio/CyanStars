using UnityEngine;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.Camera.Timeline
{
    /// <summary>
    /// 相机轨道
    /// </summary>
    public class CameraTrack : BaseTrack
    {
        public Vector3 DefaultCameraPos;
        public Transform CameraTrans;

        /// <summary>
        /// 创建相机轨道片段
        /// </summary> 
        public static readonly IClipCreator<CameraTrack, IList<CameraControllerSo.KeyFrame>> ClipCreator =
            new CameraClipCreator();

        private sealed class CameraClipCreator : IClipCreator<CameraTrack, IList<CameraControllerSo.KeyFrame>>
        {
            public BaseClip<CameraTrack> CreateClip(CameraTrack track, int clipIndex,
                IList<CameraControllerSo.KeyFrame> keyFrames)
            {
                CameraControllerSo.KeyFrame keyFrame = keyFrames[clipIndex];

                float startTime = 0;
                if (clipIndex > 0)
                {
                    startTime = keyFrames[clipIndex - 1].time;
                }

                CameraClip clip = new CameraClip(startTime / 1000f, keyFrame.time / 1000f, track, keyFrame.position,
                    keyFrame.rotation, keyFrame.smoothType);

                return clip;
            }
        }
    }
}
