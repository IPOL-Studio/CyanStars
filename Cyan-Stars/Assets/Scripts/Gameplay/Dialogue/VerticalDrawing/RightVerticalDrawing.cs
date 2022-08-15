// using TMPro;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using DG.Tweening;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace CyanStars.Dialogue
// {
//     public class RightVerticalDrawing : Image, ISetVerticalDrawing
//     {
//         private RectTransform fatherRectTransform;
//         private float width;
//         private float height;
//         private Tweener tweener;
//         private Tweener tweener2;
//         protected override void Start()
//         {
//             fatherRectTransform = DialogueManager.Instance.rectTransform;
//             var rect = rectTransform.rect;
//             width = rect.width;
//             height = rect.height;
//             DialogueManager.Instance.OnSwitchDialog += SetImage;
//             DialogueManager.Instance.OnAnimation += SetAnimation;
//             DialogueManager.Instance.OnAnimation += SetXAxisMovement;
//             DialogueManager.Instance.OnSkipAnimation += SkipAnimation;
//         }
//
//         private void AddStateCount()
//         {
//             DialogueManager.Instance.stateCount++;
//         }
//
//         private void SubStateCount()
//         {
//             DialogueManager.Instance.stateCount--;
//         }
//
//         public void SetImage(int index)
//         {
//             if(DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.file == "") return;
//             sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[index]
//                 .rightVerticalDrawings.file];
//         }
//
//         public void SetAnimation(int index)
//         {
//             if (DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.effect == "") return;
//
//             float duration = DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.time;
//             string curve = DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.curve;
//
//             switch (DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.effect)
//             {
//                 case "抖动":
//                     tweener2 = rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
//                     break;
//                 case "旋转抖动":
//                     tweener2 = rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
//                     break;
//                 case "缩放":
//                     tweener2 = rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
//                     break;
//                 default:
//                     break;
//             }
//         }
//
//         public void SetXAxisMovement(int index)
//         {
//             if (DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.xAxisMovement < 0) return;
//             float xAxisMovement = DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.xAxisMovement;
//             float duration = DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.time;
//             string curve = DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.curve;
//             tweener = rectTransform.DOAnchorPosX(-fatherRectTransform.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
//         }
//
//         public void SkipAnimation(int index)
//         {
//             tweener2.Kill(true);
//             rectTransform.DOScale(new Vector3(1, 1, 1), 0.1f);
//             rectTransform.DOSizeDelta(new Vector2(width, height), 0.1f);
//
//             if (DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.xAxisMovement < 0)
//             {
//                 return;
//             }
//             else
//             {
//                 tweener.Kill(true);
//                 rectTransform.anchoredPosition = new Vector2(-fatherRectTransform.sizeDelta.x * DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.xAxisMovement, rectTransform.anchoredPosition.y);
//             }
//         }
//
//
//     }
// }
//
