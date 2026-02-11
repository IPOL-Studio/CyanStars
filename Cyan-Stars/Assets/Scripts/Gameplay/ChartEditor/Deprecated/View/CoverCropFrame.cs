// #nullable enable
//
// using UnityEngine;
// using UnityEngine.EventSystems;
//
// namespace CyanStars.GamePlay.ChartEditor.View
// {
//     public class CoverCropFrame : BaseView, IDragHandler
//     {
//         [SerializeField]
//         private RectTransform imageFrameRect = null!;
//
//         [SerializeField]
//         private Canvas mainCanva = null!;
//
//         public void OnDrag(PointerEventData eventData)
//         {
//             if (imageFrameRect.rect.width == 0 || imageFrameRect.rect.height == 0)
//             {
//                 return;
//             }
//
//             float xRatioDelta = eventData.delta.x / imageFrameRect.rect.width / mainCanva.scaleFactor;
//             float yRatioDelta = eventData.delta.y / imageFrameRect.rect.height / mainCanva.scaleFactor;
//             Vector2 deltaPositionRatio = new Vector2(xRatioDelta, yRatioDelta);
//             Model.UpdateCoverCropByFrame(deltaPositionRatio);
//         }
//     }
// }



