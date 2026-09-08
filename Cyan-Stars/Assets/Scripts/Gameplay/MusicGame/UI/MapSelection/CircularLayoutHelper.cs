#nullable enable

using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 环形/椭圆排列的纯计算工具
    /// </summary>
    public static class CircularLayoutHelper
    {
        /// <summary>
        /// 以 ViewPort + 2 倍 item 宽高为修正矩形，修正矩形左上角为原点，右下为正方向，根据 item 锚点在修正矩形的 y 位置，计算并设置锚点对应的 x 位置
        /// </summary>
        public static void SetItemXPos(RectTransform itemRect, RectTransform viewPortRect, float subItemWidth)
        {
            float itemH = itemRect.rect.height;
            Rect vpRect = viewPortRect.rect;

            float yTop = vpRect.yMax + itemH / 2;
            float yBottom = vpRect.yMin - itemH / 2;
            float xLeft = vpRect.xMin;
            float xRight = vpRect.xMax - subItemWidth / 2;

            Vector3 itemPosInVp = viewPortRect.InverseTransformPoint(itemRect.position);

            float totalH = yTop - yBottom;
            if (Mathf.Approximately(totalH, 0f) || totalH < 0f)
                return;

            float t = (yTop - itemPosInVp.y) / totalH;
            t = Mathf.Clamp01(t);

            float curveT = Mathf.Sin(Mathf.PI * t);
            float targetXInVp = Mathf.LerpUnclamped(xLeft, xRight, curveT);

            Vector3 targetWorldPos = viewPortRect.TransformPoint(new Vector3(targetXInVp, itemPosInVp.y, itemPosInVp.z));
            Vector3 targetPosInParent = itemRect.parent != null
                ? itemRect.parent.InverseTransformPoint(targetWorldPos)
                : targetWorldPos;

            itemRect.localPosition = new Vector3(targetPosInParent.x, itemRect.localPosition.y, itemRect.localPosition.z);
        }
    }
}
