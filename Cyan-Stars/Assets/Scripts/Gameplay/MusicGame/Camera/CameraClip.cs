using UnityEngine;
using CyanStars.Framework.Timeline;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 相机片段
    /// </summary>
    public class CameraClip : BaseClip<CameraTrack>
    {
        private Vector3 position;
        private Vector3 rotation;
        private EasingFunctionType easingType;

        private Transform camTrans;
        private float length;

        private Vector3 newPos;
        private Vector3 newRot;
        private Vector3 oldPos;
        private Vector3 oldRot;

        public CameraClip(float startTime, float endTime, CameraTrack owner, Vector3 position, Vector3 rotation, EasingFunctionType easingType) : base(startTime, endTime, owner)
        {
            this.position = position;
            this.rotation = rotation;
            this.easingType = easingType;
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

            switch (easingType) //缓动
            {
                case EasingFunctionType.Linear:
                    camTrans.position = EasingFunction.LinearFunction(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles = EasingFunction.LinearFunction(oldRot, newRot, localTimer, length);
                    break;
                case EasingFunctionType.SineaseIn:
                    camTrans.position = EasingFunction.SinFunctionEaseIn(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles = EasingFunction.SinFunctionEaseIn(oldRot, newRot, localTimer, length);
                    break;
                case EasingFunctionType.SineaseOut:
                    camTrans.position = EasingFunction.SinFunctionEaseOut(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles = EasingFunction.SinFunctionEaseOut(oldRot, newRot, localTimer, length);
                    break;
                case EasingFunctionType.SineaseInOut:
                    camTrans.position = EasingFunction.SinFunctionEaseInOut(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles =
                        EasingFunction.SinFunctionEaseInOut(oldRot, newRot, localTimer, length);
                    break;
                case EasingFunctionType.BackeaseIn:
                    camTrans.position = EasingFunction.BackEaseIn(oldPos, newPos, localTimer, length);
                    camTrans.localEulerAngles = EasingFunction.BackEaseIn(oldRot, newRot, localTimer, length);
                    break;
            }
        }
    }
}
