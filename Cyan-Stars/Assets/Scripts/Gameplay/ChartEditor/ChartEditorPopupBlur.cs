#nullable enable

using CyanStars.Graphics.UI;
using DG.Tweening;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 制谱器弹窗背景模糊控制器
    /// </summary>
    /// <remarks>
    /// 通过全局 shader 变量驱动 <see cref="UIPopupBlurFeature"/>：<br/>
    /// 弹窗打开时让「面板范围内」的背景逐渐模糊，关闭时恢复清晰。<br/>
    /// 模糊范围由面板背景图自身的 alpha 决定，因此不会糊到面板以外。<br/>
    /// 当同时存在多个可见弹窗时，只有最后一个弹窗关闭后才会恢复清晰。
    /// </remarks>
    public static class ChartEditorPopupBlur
    {
        /// <summary>
        /// 每一趟高斯的抽头范围，单位为「半分辨率像素」
        /// </summary>
        private const float PassRadius = 5f;

        /// <summary>
        /// 与弹窗开关动画保持一致的渐变时长
        /// </summary>
        private const float FadeDuration = 0.2f;

        /// <summary>
        /// 面板 alpha 到遮罩的映射区间：低于下限视为面板外，高于上限视为面板内
        /// </summary>
        private static readonly Vector2 MaskRange = new(0.6f, 0.9f);

        private static readonly Vector4 EmptyRect = new(0f, 0f, 0f, 0f);
        private static readonly Vector4 FullUV = new(0f, 0f, 1f, 1f);

        private static int visiblePopupCount;
        private static float currentRadius;
        private static Tween? fadeTween;

        private static object? panelOwner;


        /// <summary>
        /// 通知一个弹窗的可见性变化
        /// </summary>
        public static void NotifyPopupVisibilityChanged(bool visible)
        {
            visiblePopupCount = Mathf.Max(0, visiblePopupCount + (visible ? 1 : -1));
            FadeTo(visiblePopupCount > 0 ? PassRadius : 0f);
        }

        /// <summary>
        /// 每帧同步当前面板的屏幕矩形与遮罩贴图
        /// </summary>
        /// <param name="owner">调用方，用于避免多个弹窗之间互相覆盖遮罩</param>
        /// <param name="panel">面板 RectTransform，坐标会被当作屏幕像素处理</param>
        /// <param name="mask">面板贴图，其 alpha 通道作为遮罩</param>
        /// <param name="maskUV">面板贴图在 mask 纹理中的 uv 范围 (uMin, vMin, uSize, vSize)</param>
        public static void UpdatePanel(object owner, RectTransform panel, Texture? mask, Vector4 maskUV)
        {
            if (panel == null || mask == null)
                return;

            panelOwner = owner;

            var corners = new Vector3[4];
            panel.GetWorldCorners(corners); // 弹窗画布是 Overlay，世界坐标即屏幕像素坐标

            float minX = Mathf.Min(corners[0].x, corners[2].x);
            float minY = Mathf.Min(corners[0].y, corners[2].y);
            float maxX = Mathf.Max(corners[0].x, corners[2].x);
            float maxY = Mathf.Max(corners[0].y, corners[2].y);

            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);

            Shader.SetGlobalVector(
                UIPopupBlurFeature.PanelRectId,
                new Vector4(minX / width, minY / height, maxX / width, maxY / height)
            );
            Shader.SetGlobalVector(UIPopupBlurFeature.PanelUVId, maskUV);
            Shader.SetGlobalTexture(UIPopupBlurFeature.PanelMaskId, mask);
        }

        /// <summary>
        /// 清除面板遮罩
        /// </summary>
        public static void ClearPanel(object owner)
        {
            if (!ReferenceEquals(panelOwner, owner))
                return;

            panelOwner = null;

            Shader.SetGlobalVector(UIPopupBlurFeature.PanelRectId, EmptyRect);
            Shader.SetGlobalVector(UIPopupBlurFeature.PanelUVId, FullUV);
        }

        /// <summary>
        /// 立即清除模糊效果
        /// </summary>
        public static void Reset()
        {
            visiblePopupCount = 0;
            KillTween();
            currentRadius = 0f;
            Shader.SetGlobalFloat(UIPopupBlurFeature.BlurRadiusId, 0f);
        }


        private static void FadeTo(float targetRadius)
        {
            KillTween();

            // 遮罩参数只在模糊生效期间有意义，这里一并写入
            Shader.SetGlobalVector(UIPopupBlurFeature.MaskRangeId, MaskRange);

            if (Mathf.Approximately(currentRadius, targetRadius))
            {
                ApplyRadius(targetRadius);
                return;
            }

            fadeTween = DOTween
                .To(() => currentRadius, ApplyRadius, targetRadius, FadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() => fadeTween = null)
                .OnKill(() => fadeTween = null);
        }

        private static void KillTween()
        {
            fadeTween?.Kill();
            fadeTween = null;
        }

        private static void ApplyRadius(float radius)
        {
            currentRadius = radius;
            Shader.SetGlobalFloat(UIPopupBlurFeature.BlurRadiusId, radius);
        }
    }
}
