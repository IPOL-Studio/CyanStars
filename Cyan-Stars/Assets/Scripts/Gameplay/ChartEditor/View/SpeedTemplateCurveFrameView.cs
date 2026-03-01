#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Model;
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


        [SerializeField]
        private GameObject pointFrameObject = null!;


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


        // --- 贝塞尔点 VM 和 V ---
        /// <summary>
        /// 用于在选中的变速模板变化时断开旧集合监听
        /// </summary>
        private readonly CompositeDisposable CollectionDisposables = new();

        /// <summary>
        /// 记录 VM 和物体映射关系
        /// </summary>
        private readonly Dictionary<SpeedTemplateBezierPointHandleItemViewModel, GameObject> SpawnedPoints = new();


        public override void Bind(SpeedTemplateCurveFrameViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            verticalRangeSliderFrame.OnValueChanged.AddListener(OnVerticalChanged);
            horizontalRangeSliderFrame.OnValueChanged.AddListener(OnHorizontalChanged);

            // 当选择了新的变速模板，或贝塞尔曲线内部数据变化时，重新烘焙并重绘速度曲线和位移曲面
            ViewModel.SelectedSpeedTemplateData
                .Select(selectedSpeedTemplate =>
                    {
                        if (selectedSpeedTemplate?.BezierCurves.Points == null)
                            return Observable.Empty<Unit>();

                        return selectedSpeedTemplate.BezierCurves.Points
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

            // 当选择了新的变速模板时，重新生成全部的贝塞尔点 View
            ViewModel.BezierPointViewModelsMap
                .Subscribe(OnViewMapChanged)
                .AddTo(this);
        }


        private void OnVerticalChanged(float lowY, float highY)
        {
            if (ViewModel.SelectedSpeedTemplateData.CurrentValue == null)
                throw new Exception("选中曲线为空时不应该调整缩放");

            scaleY = 1 / ((highY - lowY) / DefaultViewportY);
            offsetY = -((highY + lowY) / 2f);

            // 更新贝塞尔位置点 // TODO: 优化算法，只更新视窗内的贝塞尔点
            foreach (var kvp in SpawnedPoints)
            {
                ((RectTransform)kvp.Value.transform).anchoredPosition = new Vector2(
                    (kvp.Key.BezierPoint.Point.CurrentValue.PositionPoint.MsTime + offsetX) * scaleX,
                    (kvp.Key.BezierPoint.Point.CurrentValue.PositionPoint.Value + offsetY) * scaleY
                );
            }

            // 更新速度曲线和位移曲面
            DrawCurve();
        }

        private void OnHorizontalChanged(float lowX, float highX)
        {
            if (ViewModel.SelectedSpeedTemplateData.CurrentValue == null)
                throw new Exception("选中曲线为空时不应该调整缩放");

            scaleX = (highX - lowX) / DefaultViewportX;
            offsetX = lowX;

            // 更新贝塞尔位置点 // TODO: 优化算法，只更新视窗内的贝塞尔点
            foreach (var kvp in SpawnedPoints)
            {
                ((RectTransform)kvp.Value.transform).anchoredPosition = new Vector2(
                    (kvp.Key.BezierPoint.Point.CurrentValue.PositionPoint.MsTime + offsetX) * scaleX,
                    (kvp.Key.BezierPoint.Point.CurrentValue.PositionPoint.Value + offsetY) * scaleY
                );
            }

            // 更新速度曲线和位移曲面
            SpeedTemplateHelper.Bake(
                ViewModel.SelectedSpeedTemplateData.CurrentValue.ToSpeedTemplateData(),
                1f,
                out tempBakedSpeedList,
                out tempBakedDistanceList
            );

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
                    (i * SpeedTemplateHelper.SampleIntervalMsTime + offsetX) * scaleX,
                    (tempBakedSpeedList[i] + offsetY) * scaleY
                );

                distancePoints[i] = new Vector2(
                    (i * SpeedTemplateHelper.SampleIntervalMsTime + offsetX) * scaleX,
                    (tempBakedDistanceList[i] + offsetY) * scaleY
                );
            }

            speedCurveRenderer.Points = speedPoints;
            distanceCurveRenderer.Points = distancePoints;
        }

        private void OnViewMapChanged(ISynchronizedView<BezierPointWrapperModel, SpeedTemplateBezierPointHandleItemViewModel>? viewModelMap)
        {
            // 断开集合监听并销毁旧 go
            CollectionDisposables.Clear();

            foreach (var kvp in SpawnedPoints)
            {
                Destroy(kvp.Value);
            }

            SpawnedPoints.Clear();

            // 未选中变速模板时直接返回
            if (viewModelMap == null)
                return;

            // 实例化新选中的变速模板中已存在的贝塞尔点、绑定并记录
            foreach (var viewModel in viewModelMap)
            {
                GameObject go = Instantiate(bezierPointHandlePrefab, pointFrameObject.transform);
                RectTransform rectTransform = (RectTransform)go.transform;
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(0f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(
                    (viewModel.BezierPoint.Point.CurrentValue.PositionPoint.MsTime + offsetX) * scaleX,
                    (viewModel.BezierPoint.Point.CurrentValue.PositionPoint.Value + offsetY) * scaleY
                );
                go.GetComponent<SpeedTemplateBezierPointHandleItemView>().Bind(viewModel);
                SpawnedPoints[viewModel] = go;
            }

            // 为新的贝塞尔曲线列表创建响应监听
            viewModelMap.ObserveAdd()
                .Subscribe(e =>
                    {
                        GameObject go = Instantiate(bezierPointHandlePrefab, pointFrameObject.transform);
                        RectTransform rectTransform = (RectTransform)go.transform;
                        rectTransform.anchorMin = new Vector2(0f, 0.5f);
                        rectTransform.anchorMax = new Vector2(0f, 0.5f);
                        rectTransform.anchoredPosition = new Vector2(
                            (e.Value.View.BezierPoint.Point.CurrentValue.PositionPoint.MsTime + offsetX) * scaleX,
                            (e.Value.View.BezierPoint.Point.CurrentValue.PositionPoint.Value + offsetY) * scaleY
                        );
                        go.GetComponent<SpeedTemplateBezierPointHandleItemView>().Bind(e.Value.View);
                        SpawnedPoints[e.Value.View] = go;
                    }
                )
                .AddTo(CollectionDisposables);
            viewModelMap.ObserveRemove()
                .Subscribe(e =>
                    {
                        GameObject go = SpawnedPoints[e.Value.View];
                        Destroy(go);
                        SpawnedPoints.Remove(e.Value.View);
                    }
                )
                .AddTo(CollectionDisposables);
            viewModelMap.ObserveReplace()
                .Subscribe(e =>
                    {
                        GameObject oldGO = SpawnedPoints[e.OldValue.View];
                        Destroy(oldGO);
                        SpawnedPoints.Remove(e.OldValue.View);

                        GameObject newGO = Instantiate(bezierPointHandlePrefab, pointFrameObject.transform);
                        RectTransform rectTransform = (RectTransform)newGO.transform;
                        rectTransform.anchorMin = new Vector2(0f, 0.5f);
                        rectTransform.anchorMax = new Vector2(0f, 0.5f);
                        rectTransform.anchoredPosition = new Vector2(
                            (e.NewValue.View.BezierPoint.Point.CurrentValue.PositionPoint.MsTime + offsetX) * scaleX,
                            (e.NewValue.View.BezierPoint.Point.CurrentValue.PositionPoint.Value + offsetY) * scaleY
                        );
                        newGO.GetComponent<SpeedTemplateBezierPointHandleItemView>().Bind(e.NewValue.View);
                        SpawnedPoints[e.NewValue.View] = newGO;
                    }
                )
                .AddTo(CollectionDisposables);
        }


        private void OnDestroy()
        {
            verticalRangeSliderFrame.OnValueChanged.RemoveListener(OnVerticalChanged);
            horizontalRangeSliderFrame.OnValueChanged.RemoveListener(OnHorizontalChanged);
        }
    }
}
