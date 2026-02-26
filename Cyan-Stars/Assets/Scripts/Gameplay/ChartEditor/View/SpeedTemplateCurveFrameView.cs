#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 变速模板-曲线编辑区域 View
    /// </summary>
    /// <remarks>
    /// - 负责绘制速度曲线和位移曲面
    /// - 负责动态生成、销毁贝塞尔点，并管理其位置和控制点显隐
    /// </remarks>
    public class SpeedTemplateCurveFrameView : BaseView<SpeedTemplateCurveFrameViewModel>
    {
        [SerializeField]
        private GameObject bezierPointHandlePrefab = null!;


        [SerializeField]
        private UILineRenderer speedCurveRenderer = null!;

        [SerializeField]
        private UILineRenderer distanceCurveRenderer = null!;

        [SerializeField]
        private RangeSlider verticalRangeSliderFrame = null!;

        [SerializeField]
        private RangeSlider horizontalRangeSliderFrame = null!;


        // --- 速度和位移曲线绘制 ---

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


        // --- 贝塞尔点位置和显隐 ---

        private readonly List<SpeedTemplateBezierPointHandleItem> BezierPointHandles = new List<SpeedTemplateBezierPointHandleItem>();


        public override void Bind(SpeedTemplateCurveFrameViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            verticalRangeSliderFrame.OnValueChanged.AddListener(OnVerticalChanged);
            horizontalRangeSliderFrame.OnValueChanged.AddListener(OnHorizontalChanged);

            // 当选择了新的变速模板，或贝塞尔曲线内部数据变化时，重新烘焙并重绘曲线
            ViewModel.SelectedSpeedTemplateData
                .Select(speedTemplateData =>
                    {
                        if (speedTemplateData?.BezierCurves?.Points == null)
                            return Observable.Empty<Unit>();

                        return speedTemplateData.BezierCurves.Points
                            .ObserveChanged()
                            .Select(_ => Unit.Default)
                            .Prepend(Unit.Default);
                    }
                )
                .Switch()
                .Subscribe(_ =>
                    {
                        if (ViewModel.SelectedSpeedTemplateData.CurrentValue == null)
                            throw new Exception("选中曲线为空时应当丢弃通知流");

                        SpeedTemplateHelper.Bake(
                            ViewModel.SelectedSpeedTemplateData.CurrentValue.ToSpeedTemplateData(),
                            1f,
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
                throw new Exception("选中曲线为空时不应该调整缩放");

            SpeedTemplateHelper.Bake(
                ViewModel.SelectedSpeedTemplateData.CurrentValue.ToSpeedTemplateData(),
                1f,
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

            if (scaleX == 0 || scaleY == 0)
            {
                // 缩放为 0 时不绘制曲线，防止除 0 异常
                speedCurveRenderer.Points = Array.Empty<Vector2>();
                distanceCurveRenderer.Points = Array.Empty<Vector2>();
                return;
            }

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
