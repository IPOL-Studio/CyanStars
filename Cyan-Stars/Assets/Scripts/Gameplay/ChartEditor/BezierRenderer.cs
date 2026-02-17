#nullable enable

using System.Collections.Generic;
using CyanStars.Utils.SpeedTemplate;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.ChartEditor
{
    /// <summary>
    /// 用于变速模板的贝塞尔曲线渲染器
    /// </summary>
    public class BezierRenderer : MaskableGraphic
    {
        private const float Width = 2f; // 线条宽度
        private static readonly Color SpeedCurveColor = Color.forestGreen;


        public void DrawSpeedCurve(ICollection<BezierPoint> speedPoints)
        {
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
        }
    }
}
