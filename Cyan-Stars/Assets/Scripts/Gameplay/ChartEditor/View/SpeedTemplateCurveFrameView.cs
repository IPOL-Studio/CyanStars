#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils.SpeedTemplate;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class SpeedTemplateCurveFrameView : BaseView<SpeedTemplateCurveFrameViewModel>
    {
        [SerializeField]
        private UILineRenderer speedCurveRenderer = null!;

        [SerializeField]
        private UILineRenderer distanceCurveRenderer = null!;


        // 用于控制当前编辑区域的位置和缩放
        private const float DefaultMinX = 0f;
        private const float DefaultMaxX = 1000f;
        private const float DefaultMinY = -200f;
        private const float DefaultMaxY = 200f;

        private float viewportMinX = DefaultMinX;
        private float viewportMaxX = DefaultMaxX;
        private float viewportMinY = DefaultMinY;
        private float viewportMaxY = DefaultMaxY;

        private float OffsetX => (viewportMaxX + viewportMinX) / 2f - (DefaultMaxX + DefaultMinX) / 2f;
        private float OffsetY => (viewportMaxY + viewportMinY) / 2f - (DefaultMaxY + DefaultMinY) / 2f;
        private float ScaleX => (viewportMaxX - viewportMinX) / (DefaultMaxX - DefaultMinX);
        private float ScaleY => (viewportMaxY - viewportMinY) / (DefaultMaxY - DefaultMinY);


        public override void Bind(SpeedTemplateCurveFrameViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            BezierCurve bezierCurve = new BezierCurve(
                new BezierPoint(
                    new BezierPointPos(0, 0),
                    new BezierPointPos(0, 0),
                    new BezierPointPos(0, 0)
                )
            )
            {
                new BezierPoint(
                    new BezierPointPos(500, 100),
                    new BezierPointPos(200, 100),
                    new BezierPointPos(800, 100)
                ),
                new BezierPoint(
                    new BezierPointPos(1000, 0),
                    new BezierPointPos(1000, 0),
                    new BezierPointPos(1000, 0)
                )
            };

            var std = new SpeedTemplateData(bezierCurve: bezierCurve);
            SpeedTemplateHelper.Bake(std, 1f, out List<float> speedList, out List<float> distanceList);

            Vector2[] speedPoints = new Vector2[speedList.Count - 1];
            Vector2[] distancePoints = new Vector2[distanceList.Count - 1];
            for (int i = 0; i < distanceList.Count - 1; i++)
            {
                speedPoints[i] = new Vector2(i * SpeedTemplateHelper.SampleIntervalMs, speedList[i]);
                distancePoints[i] = new Vector2(i * SpeedTemplateHelper.SampleIntervalMs, distanceList[i]);
            }

            speedCurveRenderer.Points = speedPoints;
            distanceCurveRenderer.Points = distancePoints;
        }
    }
}
