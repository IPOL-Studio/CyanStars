// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Linq;
// using CyanStars.Chart;
// using CyanStars.GamePlay.ChartEditor.Model;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace CyanStars.GamePlay.ChartEditor.View
// {
//     public class NoteAttribute : BaseView
//     {
//         // 资源文件
//         [SerializeField]
//         private Sprite selectedIcon;
//
//         [SerializeField]
//         private Sprite unselectedIcon;
//
//         // 侧边栏组件
//         [SerializeField]
//         private TMP_InputField judgeBeatField1;
//
//         [SerializeField]
//         private TMP_InputField judgeBeatField2;
//
//         [SerializeField]
//         private TMP_InputField judgeBeatField3;
//
//
//         [SerializeField]
//         private TMP_InputField endBeatField1;
//
//         [SerializeField]
//         private TMP_InputField endBeatField2;
//
//         [SerializeField]
//         private TMP_InputField endBeatField3;
//
//
//         [SerializeField]
//         private TMP_InputField posField;
//
//         [SerializeField]
//         private Toggle breakToggleL;
//
//         [SerializeField]
//         private Toggle breakToggleR;
//
//
//         [SerializeField]
//         private TMP_Dropdown correctAudioDropdown; // TODO
//
//         [SerializeField]
//         private TMP_Dropdown hitAudioDropdown; // TODO
//
//         [SerializeField]
//         private TMP_Dropdown speedGroupDropdown; // TODO
//
//         [SerializeField]
//         private TMP_InputField speedOffsetField; // TODO
//
//
//         // 子物体，用于根据选中音符类型动态控制是否显示
//         [SerializeField]
//         private GameObject judgeBeatObject;
//
//         [SerializeField]
//         private GameObject holdEndBeatObject;
//
//         [SerializeField]
//         private GameObject posObject;
//
//         [SerializeField]
//         private GameObject breakPosObject;
//
//         [SerializeField]
//         private GameObject correctAudioObject; // TODO
//
//         [SerializeField]
//         private GameObject hitAudioObject; // TODO
//
//         [SerializeField]
//         private GameObject speedGroupAudioObject; // TODO
//
//         [SerializeField]
//         private GameObject speedOffsetAudioObject; // TODO
//
//
//         public override void Bind(ChartEditorModel chartEditorModel)
//         {
//             base.Bind(chartEditorModel);
//
//             judgeBeatField1.onEndEdit.AddListener((text) => Model.SetJudgeBeat(text, null, null));
//             judgeBeatField2.onEndEdit.AddListener((text) => Model.SetJudgeBeat(null, text, null));
//             judgeBeatField3.onEndEdit.AddListener((text) => Model.SetJudgeBeat(null, null, text));
//             endBeatField1.onEndEdit.AddListener((text) => Model.SetEndBeat(text, null, null));
//             endBeatField2.onEndEdit.AddListener((text) => Model.SetEndBeat(null, text, null));
//             endBeatField3.onEndEdit.AddListener((text) => Model.SetEndBeat(null, null, text));
//             posField.onEndEdit.AddListener((text) => Model.SetPos(text));
//             breakToggleL.onValueChanged.AddListener((isOn) =>
//             {
//                 if (isOn)
//                 {
//                     Model.SetBreakPos(BreakNotePos.Left);
//                 }
//             });
//             breakToggleR.onValueChanged.AddListener((isOn) =>
//             {
//                 if (isOn)
//                 {
//                     Model.SetBreakPos(BreakNotePos.Right);
//                 }
//             });
//
//             Model.OnSelectedNotesChanged += RefreshNoteAttribute;
//             Model.OnNoteAttributeChanged += RefreshNoteAttribute;
//         }
//
//         /// <summary>
//         /// 当选择的音符变化时，或音符的属性变化时，刷新属性面板
//         /// </summary>
//         public void RefreshNoteAttribute()
//         {
//             // 只有选中音符，才展示 Note 属性（否则展示编辑器属性）
//             foreach (Transform child in this.transform)
//             {
//                 child.gameObject.SetActive(Model.SelectedNotes.Count > 0);
//             }
//
//             if (Model.SelectedNotes.Count == 0)
//             {
//                 return;
//             }
//
//             // 根据选中的音符，动态显示可编辑属性
//             bool hasTapNote = Model.SelectedNotes.Any(baseNoteData => baseNoteData is TapChartNoteData);
//             bool hasDragNote = Model.SelectedNotes.Any(baseNoteData => baseNoteData is DragChartNoteData);
//             bool hasHoldNote = Model.SelectedNotes.Any(baseNoteData => baseNoteData is HoldChartNoteData);
//             bool hasClickNote = Model.SelectedNotes.Any(baseNoteData => baseNoteData is ClickChartNoteData);
//             bool hasBreakNote = Model.SelectedNotes.Any(baseNoteData => baseNoteData is BreakChartNoteData);
//             judgeBeatObject.SetActive(true);
//             holdEndBeatObject.SetActive(hasHoldNote);
//             posObject.SetActive(hasTapNote || hasDragNote || hasHoldNote || hasClickNote);
//             breakPosObject.SetActive(hasBreakNote);
//             correctAudioObject.SetActive(false); // TODO
//             hitAudioObject.SetActive(false); // TODO
//             speedGroupAudioObject.SetActive(false); // TODO
//             speedOffsetAudioObject.SetActive(false); // TODO
//
//             // 查询选中音符中各属性的值，有多个值的以“-”表示，有唯一值的直接显示值
//             judgeBeatField1.text =
//                 TryGetUniquePropertyValue(Model.SelectedNotes, item => item.JudgeBeat.IntegerPart, out int judgeIntPart)
//                     ? judgeIntPart.ToString()
//                     : "-";
//             judgeBeatField2.text =
//                 TryGetUniquePropertyValue(Model.SelectedNotes, item => item.JudgeBeat.Numerator, out int judgeNumerator)
//                     ? judgeNumerator.ToString()
//                     : "-";
//             judgeBeatField3.text =
//                 TryGetUniquePropertyValue(Model.SelectedNotes, item => item.JudgeBeat.Denominator,
//                     out int judgeDenominator)
//                     ? judgeDenominator.ToString()
//                     : "-";
//
//             var selectedHoldNotes = Model.SelectedNotes.OfType<HoldChartNoteData>();
//             endBeatField1.text =
//                 TryGetUniquePropertyValue(selectedHoldNotes, item => item.EndJudgeBeat.IntegerPart, out int endIntPart)
//                     ? endIntPart.ToString()
//                     : "-";
//             endBeatField2.text =
//                 TryGetUniquePropertyValue(selectedHoldNotes, item => item.EndJudgeBeat.Numerator, out int endNumerator)
//                     ? endNumerator.ToString()
//                     : "-";
//             endBeatField3.text =
//                 TryGetUniquePropertyValue(selectedHoldNotes, item => item.EndJudgeBeat.Denominator,
//                     out int endDenominator)
//                     ? endDenominator.ToString()
//                     : "-";
//
//             var selectedIChartNoteNormalPos = Model.SelectedNotes.OfType<IChartNoteNormalPos>().ToList();
//             posField.text = TryGetUniquePropertyValue(selectedIChartNoteNormalPos, item => item.Pos,
//                 out float pos)
//                 ? pos.ToString(CultureInfo.InvariantCulture)
//                 : "-";
//
//             var selectedBreakNotes = Model.SelectedNotes.OfType<BreakChartNoteData>().ToList();
//             if (TryGetUniquePropertyValue(selectedBreakNotes, item => item.BreakNotePos, out BreakNotePos breakNotePos))
//             {
//                 if (breakNotePos == BreakNotePos.Left)
//                 {
//                     breakToggleL.isOn = true;
//                     breakToggleR.isOn = false;
//                     breakToggleL.GetComponentInChildren<Image>().sprite = selectedIcon;
//                     breakToggleR.GetComponentInChildren<Image>().sprite = unselectedIcon;
//                 }
//                 else
//                 {
//                     breakToggleL.isOn = false;
//                     breakToggleR.isOn = true;
//                     breakToggleL.GetComponentInChildren<Image>().sprite = unselectedIcon;
//                     breakToggleR.GetComponentInChildren<Image>().sprite = selectedIcon;
//                 }
//             }
//             else
//             {
//                 breakToggleL.isOn = false;
//                 breakToggleR.isOn = false;
//                 breakToggleL.GetComponentInChildren<Image>().sprite = unselectedIcon;
//                 breakToggleR.GetComponentInChildren<Image>().sprite = unselectedIcon;
//             }
//         }
//
//         /// <summary>
//         /// 尝试从一个集合中获取某个属性的唯一值。
//         /// </summary>
//         /// <typeparam name="TTarget">集合中元素的类型。</typeparam>
//         /// <typeparam name="TResult">要检查的属性的类型。</typeparam>
//         /// <param name="source">要检查的源集合。</param>
//         /// <param name="selector">一个用于从元素中提取属性的函数。</param>
//         /// <param name="value">如果找到唯一值，则通过此参数传出；否则传出该类型的默认值。</param>
//         /// <returns>如果集合中所有元素的该属性值都相同且集合不为空，则返回 true；否则返回 false。</returns>
//         private bool TryGetUniquePropertyValue<TTarget, TResult>(
//             IEnumerable<TTarget> source,
//             Func<TTarget, TResult> selector,
//             out TResult value)
//         {
//             value = default(TResult);
//
//             if (source == null || !source.Any())
//             {
//                 return false;
//             }
//
//             var distinctValues = source.Select(selector).Distinct().Take(2).ToList();
//             if (distinctValues.Count == 1)
//             {
//                 value = distinctValues[0];
//                 return true;
//             }
//
//             return false;
//         }
//
//         private void OnDestroy()
//         {
//             Model.OnSelectedNotesChanged -= RefreshNoteAttribute;
//             Model.OnNoteAttributeChanged -= RefreshNoteAttribute;
//         }
//     }
// }



