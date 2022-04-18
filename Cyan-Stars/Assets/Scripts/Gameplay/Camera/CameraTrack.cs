using UnityEngine;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Data;

namespace CyanStars.Gameplay.Camera
{
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
        public static readonly IClipCreator<CameraTrack, CameraTrackData> ClipCreator =
            new CameraClipCreator();

        private sealed class CameraClipCreator : IClipCreator<CameraTrack, CameraTrackData>
        {
            public BaseClip<CameraTrack> CreateClip(CameraTrack track, int clipIndex,
                CameraTrackData data)
            {
                CameraTrackData.KeyFrame keyFrame = data.KeyFrames[clipIndex];

                float startTime = 0;
                if (clipIndex > 0)
                {
                    startTime = data.KeyFrames[clipIndex - 1].Time;
                }

                CameraClip clip = new CameraClip(startTime / 1000f, keyFrame.Time / 1000f, track, keyFrame.Position,
                    keyFrame.Rotation, keyFrame.SmoothType);

                return clip;
            }
        }
    }
}
