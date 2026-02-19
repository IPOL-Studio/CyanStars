#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils.SpeedTemplate;
using R3;
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

        [SerializeField]
        private RangeSlider verticalRangeSliderFrame = null!;

        [SerializeField]
        private RangeSlider horizontalRangeSliderFrame = null!;


        private const float DefaultViewportX = 1000f;
        private const float DefaultViewportY = 400f;

        private float scaleX = 1f;
        private float scaleY = 1f;
        private float offsetX = 0f;
        private float offsetY = 0f;

        // 缓存的从已烘焙的坐标，当 x 视界大小变化或视界内贝塞尔点属性变化时需要重新烘焙
        // 视界左侧之外的曲线不会参与此处烘焙
        private List<float> tempBakedSpeedList = new List<float>();
        private List<float> tempBakedDistanceList = new List<float>();


        public override void Bind(SpeedTemplateCurveFrameViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            verticalRangeSliderFrame.OnValueChanged.AddListener(OnVerticalChanged);
            horizontalRangeSliderFrame.OnValueChanged.AddListener(OnHorizontalChanged);

            // 当选择了新的变速模板，或贝塞尔曲线内部数据变化时，重新烘焙并重绘曲线
            Observable.Merge(
                    ViewModel.SelectedSpeedTemplateData
                        .Select(speedTemplate => speedTemplate?.BezierCurves),
                    ViewModel.SelectedBezierCurvePropertyUpdatedSubject
                        .Select(data => (BezierCurves?)data)
                )
                .Subscribe(data =>
                    {
                        if (data == null || ViewModel.SelectedSpeedTemplateData.CurrentValue == null)
                            return;

                        SpeedTemplateHelper.Bake(
                            ViewModel.SelectedSpeedTemplateData.CurrentValue,
                            1f,
                            (int)horizontalRangeSliderFrame.HighValue,
                            out tempBakedSpeedList,
                            out tempBakedDistanceList
                        );

                        DrawCurve();
                    }
                )
                .AddTo(this);
        }


        private void OnVerticalChanged(float lowY, float highY)
        {
            scaleY = (highY - lowY) / DefaultViewportY;
            offsetY = (highY + lowY) / 2f;
            DrawCurve();
        }

        private void OnHorizontalChanged(float lowX, float highX)
        {
            if (ViewModel.SelectedSpeedTemplateData.CurrentValue == null)
                return;

            SpeedTemplateHelper.Bake(
                ViewModel.SelectedSpeedTemplateData.CurrentValue,
                1f,
                (int)horizontalRangeSliderFrame.HighValue,
                out tempBakedSpeedList,
                out tempBakedDistanceList
            );

            scaleX = (highX - lowX) / DefaultViewportX;
            offsetX = lowX;

            DrawCurve();
        }


        /// <summary>
        /// 根据已烘焙的曲线列表来绘制曲线
        /// </summary>
        private void DrawCurve()
        {
            Vector2[] speedPoints = new Vector2[tempBakedSpeedList.Count];
            Vector2[] distancePoints = new Vector2[tempBakedDistanceList.Count];

            for (int i = 0; i < tempBakedSpeedList.Count; i++)
            {
                speedPoints[i] = new Vector2(
                    (i * SpeedTemplateHelper.SampleIntervalMsTime - offsetX) / scaleX,
                    (tempBakedSpeedList[i] - offsetY) / scaleY
                );

                distancePoints[i] = new Vector2(
                    (i * SpeedTemplateHelper.SampleIntervalMsTime - offsetX) / scaleX,
                    (tempBakedDistanceList[i] - offsetY) / scaleY
                );
            }

            speedCurveRenderer.Points = speedPoints;
            distanceCurveRenderer.Points = distancePoints;
        }


        private void OnDestroy()
        {
            verticalRangeSliderFrame.OnValueChanged.RemoveListener(OnVerticalChanged);
            horizontalRangeSliderFrame.OnValueChanged.RemoveListener(OnHorizontalChanged);
        }
    }
}
