// using System.Globalization;
// using CyanStars.Chart;
// using CyanStars.GamePlay.ChartEditor.Model;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace CyanStars.GamePlay.ChartEditor.View
// {
//     public class BpmGroupCanvas : BaseView
//     {
//         [SerializeField]
//         private Button closeCanvasButton;
//
//         [SerializeField]
//         private Canvas canvas;
//
//         [Header("进度条")]
//         [SerializeField]
//         private GameObject progressBarObject; // TODO
//
//         [Header("列表区域")]
//         [SerializeField]
//         private GameObject listObject;
//
//         [SerializeField]
//         private GameObject contentObject;
//
//         [SerializeField]
//         private Button addItemButton;
//
//         [SerializeField]
//         private GameObject bpmItemPrefab;
//
//
//         [Header("数据编辑区域")]
//         [SerializeField]
//         private TMP_Text titleText;
//
//         [SerializeField]
//         private TMP_InputField bpmValueField;
//
//         [SerializeField]
//         private Button measureBpmButton;
//
//
//         [SerializeField]
//         private TMP_InputField startBeatField1;
//
//         [SerializeField]
//         private TMP_InputField startBeatField2;
//
//         [SerializeField]
//         private TMP_InputField startBeatField3;
//
//
//         [SerializeField]
//         private Button deleteItemButton;
//
//         [SerializeField]
//         private Toggle alarmToggle;
//
//         [SerializeField]
//         private Button testPlayButton;
//
//         public override void Bind(ChartEditorModel chartEditorModel)
//         {
//             base.Bind(chartEditorModel);
//
//             Model.OnBpmGroupCanvasVisiblenessChanged += RefreshUI;
//             Model.OnBpmGroupChanged += RefreshUI;
//             Model.OnSelectedBpmItemChanged += RefreshUI;
//
//             closeCanvasButton.onClick.AddListener(() => { Model.SetBpmGroupCanvasVisibleness(false); });
//             addItemButton.onClick.AddListener(() => { Model.AddBpmItem(); });
//             deleteItemButton.onClick.AddListener(() => { Model.DeleteBpmItem(); });
//
//             bpmValueField.onEndEdit.AddListener((text) => { Model.UpdateBpmItemBpm(text); });
//             startBeatField1.onEndEdit.AddListener((_) =>
//             {
//                 Model.UpdateBpmItemStartBeat(startBeatField1.text, startBeatField2.text, startBeatField3.text);
//             });
//             startBeatField2.onEndEdit.AddListener((_) =>
//             {
//                 Model.UpdateBpmItemStartBeat(startBeatField1.text, startBeatField2.text, startBeatField3.text);
//             });
//             startBeatField3.onEndEdit.AddListener((_) =>
//             {
//                 Model.UpdateBpmItemStartBeat(startBeatField1.text, startBeatField2.text, startBeatField3.text);
//             });
//
//             RefreshUI();
//         }
//
//         private void RefreshUI()
//         {
//             canvas.enabled = Model.BpmGroupCanvasVisibleness;
//
//             // 简易模式下至少存在1个BPM组，没有时自动补齐至1个
//             if (Model.BpmGroupDatas.Count == 0 && Model.IsSimplification)
//             {
//                 Model.AddBpmItem();
//             }
//
//             // 如果选中的是第一个 item，不允许修改起始拍
//             startBeatField1.readOnly = startBeatField2.readOnly = startBeatField3.readOnly =
//                 (Model.SelectedBpmItemIndex != 0);
//
//             // 开启了简易模式且只有一个 item 时，不显示左侧列表和添加按钮
//             if (!Model.IsSimplification || Model.BpmGroupDatas.Count >= 2)
//             {
//                 // 删除多余元素
//                 BpmItem[] items = contentObject.GetComponentsInChildren<BpmItem>();
//                 for (int i = items.Length - 1; i >= Model.BpmGroupDatas.Count; i--)
//                 {
//                     Destroy(items[i].gameObject);
//                 }
//
//                 // 添加新元素
//                 for (int i = items.Length; i < Model.BpmGroupDatas.Count; i++)
//                 {
//                     GameObject go = Instantiate(bpmItemPrefab, contentObject.transform);
//                     go.transform.SetSiblingIndex(contentObject.transform.childCount - 2);
//                 }
//
//                 // 刷新已有元素的内容
//                 items = contentObject.GetComponentsInChildren<BpmItem>();
//                 for (int i = 0; i < Model.BpmGroupDatas.Count; i++)
//                 {
//                     items[i].InitDataAndBind(Model, i);
//                 }
//
//                 // 刷新 UI 自动布局
//                 Canvas.ForceUpdateCanvases();
//                 LayoutRebuilder.ForceRebuildLayoutImmediate(contentObject.GetComponent<RectTransform>());
//             }
//
//             if (Model.IsSimplification && Model.BpmGroupDatas.Count <= 1)
//             {
//                 listObject.SetActive(false);
//             }
//             else
//             {
//                 listObject.SetActive(true);
//             }
//
//             if (Model.SelectedBpmItemIndex != null)
//             {
//                 BpmGroupItem bpmGroupItem = Model.BpmGroupDatas[(int)Model.SelectedBpmItemIndex];
//                 bpmValueField.text = bpmGroupItem.Bpm.ToString(CultureInfo.InvariantCulture);
//                 startBeatField1.text = bpmGroupItem.StartBeat.IntegerPart.ToString();
//                 startBeatField2.text = bpmGroupItem.StartBeat.Numerator.ToString();
//                 startBeatField3.text = bpmGroupItem.StartBeat.Denominator.ToString();
//             }
//             else
//             {
//                 // TODO: 未选中 bpmItem 时改一下 UI
//             }
//         }
//
//         private void OnDestroy()
//         {
//             Model.OnBpmGroupCanvasVisiblenessChanged -= RefreshUI;
//             Model.OnBpmGroupChanged -= RefreshUI;
//             Model.OnSelectedBpmItemChanged -= RefreshUI;
//         }
//     }
// }
