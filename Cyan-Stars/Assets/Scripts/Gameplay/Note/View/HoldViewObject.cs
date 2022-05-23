using UnityEngine;
using CyanStars.Framework.Utils;

namespace CyanStars.Gameplay.Note
{
    public class HoldViewObject : ViewObject
    {
        public void SetLength(float length)
        {
            var t = transform;
            var s = t.localScale;
            s.z = length;
            t.localScale = s;
        }
    }
}
