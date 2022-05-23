using UnityEngine;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Misc;

namespace CyanStars.Gameplay.Camera
{
    /// <summary>
    /// 相机片段
    /// </summary>
    public class CameraClip : BaseClip<CameraTrack>
    {
        private Vector3 position;
        private Vector3 rotation;
        private SmoothFunctionType smoothType;

        private Transform camTrans;
        private float length;

        private Vector3 newPos;
        private Vector3 newRot;
        private Vector3 oldPos;
        private Vector3 oldRot;

        public CameraClip(float startTime, float endTime, CameraTrack owner, Vector3 position, Vector3 rotation, SmoothFunctionType smoothType) : base(startTime, endTime, owner)
        {
            this.position = position;
            this.rotation = rotation;
            this.smoothType = smoothType;
            length = EndTime - StartTime;
        }

        public override void OnEnter()
        {
            camTrans = Owner.CameraTrans;
            newPos = Owner.DefaultCameraPos + position;
            newRot = rotation;
            oldPos = camTrans.position;
            oldRot = Owner.oldRot;
        }

        public override void OnExit()
        {
            Owner.oldRot = newRot;
            camTrans.position = newPos;
            camTrans.localEulerAngles = newRot;
        }

        public override void OnUpdate(float currentTime, float previousTime)
        {
            float localTimer = currentTime - StartTime;

            switch (smoothType) //缓动
            {
                case SmoothFunctionType.Linear:
                    camTrans.position = SmoothFunction.LinearFunction(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles = SmoothFunction.LinearFunction(oldRot, newRot, localTimer, length);
                    break;
                case SmoothFunctionType.SineaseIn:
                    camTrans.position = SmoothFunction.SinFunctionEaseIn(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles = SmoothFunction.SinFunctionEaseIn(oldRot, newRot, localTimer, length);
                    break;
                case SmoothFunctionType.SineaseOut:
                    camTrans.position = SmoothFunction.SinFunctionEaseOut(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles = SmoothFunction.SinFunctionEaseOut(oldRot, newRot, localTimer, length);
                    break;
                case SmoothFunctionType.SineaseInOut:
                    camTrans.position = SmoothFunction.SinFunctionEaseInOut(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles =
                        SmoothFunction.SinFunctionEaseInOut(oldRot, newRot, localTimer, length);
                    break;
                case SmoothFunctionType.BackeaseIn:
                    camTrans.position = SmoothFunction.BackEaseIn(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles = SmoothFunction.BackEaseIn(oldRot, newRot, localTimer, length);
                    break;
            }
        }
    }
}
