#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class BpmGroupViewModel : BaseViewModel
    {
        public readonly Observable<int> BpmGroupDataChangedSubject;


        public readonly ISynchronizedView<BpmGroupItem, BpmGroupListItemViewModel> BpmListItems;

        private readonly ReactiveProperty<BpmGroupItem?> selectedBpmItem;
        public ReadOnlyReactiveProperty<BpmGroupItem?> SelectedBpmItem => selectedBpmItem;


        public readonly ReadOnlyReactiveProperty<bool> ListVisible;

        public readonly ReadOnlyReactiveProperty<int?> SelectedBpmItemIndex;
        public readonly ReadOnlyReactiveProperty<string> BpmText;
        public readonly ReadOnlyReactiveProperty<string> StartBeatText1;
        public readonly ReadOnlyReactiveProperty<string> StartBeatText2;
        public readonly ReadOnlyReactiveProperty<string> StartBeatText3;


        public BpmGroupViewModel(ChartEditorModel model)
            : base(model)
        {
            BpmGroupDataChangedSubject = Model.BpmGroupDataChangedSubject.AsObservable();

            // TODO: 这里之后最好改一下，不要直接读取 0 号元素，在加载谱面时做个验证
            if (Model.ChartPackData.CurrentValue.BpmGroup.Count == 0)
                throw new NotImplementedException();

            selectedBpmItem = new ReactiveProperty<BpmGroupItem?>(Model.ChartPackData.CurrentValue.BpmGroup[0]);
            BpmListItems = Model.ChartPackData.CurrentValue.BpmGroup
                .CreateView(bpmItem =>
                    new BpmGroupListItemViewModel(
                        Model,
                        this,
                        bpmItem,
                        Model.ChartPackData.CurrentValue.BpmGroup
                    )
                )
                .AddTo(base.Disposables);

            ListVisible = Observable.CombineLatest(
                    Model.IsSimplificationMode,
                    Model.ChartPackData,
                    (isSimple, chartPackData) => !isSimple || (chartPackData?.BpmGroup.Count ?? 0) == 0
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);

            Model.IsSimplificationMode.ToReadOnlyReactiveProperty().AddTo(Disposables);

            SelectedBpmItemIndex = SelectedBpmItem
                .Select(item => item != null
                    ? (int?)Model.ChartPackData.CurrentValue.BpmGroup.IndexOf(item)
                    : null)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            BpmText = SelectedBpmItem
                .Select(item => item?.Bpm.ToString(CultureInfo.InvariantCulture) ?? "")
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(Disposables);
            StartBeatText1 = SelectedBpmItem
                .Select(item => item != null ? item.StartBeat.IntegerPart.ToString() : "")
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(Disposables);
            StartBeatText2 = SelectedBpmItem
                .Select(item => item != null ? item.StartBeat.Numerator.ToString() : "")
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(Disposables);
            StartBeatText3 = SelectedBpmItem
                .Select(item => item != null ? item.StartBeat.Denominator.ToString() : "")
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(Disposables);
        }


        /// <summary>
        /// 选中新 bpmItem
        /// </summary>
        public void SelectBpmItem(BpmGroupItem? newItem)
        {
            if (newItem == selectedBpmItem.Value)
                return;

            var oldItem = selectedBpmItem.Value;
            CommandStack.ExecuteCommand(new DelegateCommand(
                    () => selectedBpmItem.Value = newItem,
                    () => selectedBpmItem.Value = oldItem
                )
            );
        }

        /// <summary>
        /// 在列表末尾新增一个 bpmItem
        /// </summary>
        /// <remarks>新增的 bpmItem 的 startBeat 默认为已有的最后一个 bpmItem 的 startBeat + [1,0,0]；bpm 将相等</remarks>
        public void AddBpmItem()
        {
            Beat oldStartBeat = Model.ChartPackData.CurrentValue.BpmGroup[^1].StartBeat;
            float oldBpm = Model.ChartPackData.CurrentValue.BpmGroup[^1].Bpm;

            if (!Beat.TryCreateBeat(oldStartBeat.IntegerPart + 1, oldStartBeat.Numerator,
                    oldStartBeat.Denominator, out Beat newStartBeat))
                throw new Exception("无法正确构建 Beat");

            CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.BpmGroup.Add(new BpmGroupItem(oldBpm, newStartBeat));
                    },
                    () =>
                    {
                        int lastItemIndex = Model.ChartPackData.CurrentValue.BpmGroup.Count - 1;
                        Model.ChartPackData.CurrentValue.BpmGroup.RemoveAt(lastItemIndex);
                    }
                )
            );
        }


        /// <summary>
        /// 为选中的 bpmItem 设置 bpm
        /// </summary>
        public void SetBpm(string newBpmString)
        {
            if (selectedBpmItem.CurrentValue == null)
                throw new Exception("未选中 BpmItem");

            int itemIndex = Model.ChartPackData.CurrentValue.BpmGroup.IndexOf(SelectedBpmItem.CurrentValue);

            if (!float.TryParse(newBpmString, out float newBpm))
            {
                selectedBpmItem.ForceNotify();
                return;
            }

            if (Mathf.Approximately(SelectedBpmItem.CurrentValue.Bpm, newBpm))
                return;

            var oldBpm = selectedBpmItem.CurrentValue.Bpm;
            CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        selectedBpmItem.CurrentValue.Bpm = newBpm;
                        BpmGroupHelper.Sort(Model.ChartPackData.CurrentValue.BpmGroup);
                        Model.BpmGroupDataChangedSubject.OnNext(itemIndex);
                    },
                    () =>
                    {
                        selectedBpmItem.CurrentValue.Bpm = oldBpm;
                        BpmGroupHelper.Sort(Model.ChartPackData.CurrentValue.BpmGroup);
                        Model.BpmGroupDataChangedSubject.OnNext(itemIndex);
                    }
                )
            );
        }

        /// <summary>
        /// 为选中的 bpmItem 设置起始拍
        /// </summary>
        public void SetBeat(string newBeatString1, string newBeatString2, string newBeatString3)
        {
            if (selectedBpmItem.CurrentValue == null)
                throw new Exception("未选中 BpmItem");

            int itemIndex = Model.ChartPackData.CurrentValue.BpmGroup.IndexOf(SelectedBpmItem.CurrentValue);
            if (itemIndex == 0)
                throw new Exception("不允许修改首个 BpmItem 的 StartBeat");

            if (!int.TryParse(newBeatString1, out int newBeatInt1) ||
                !int.TryParse(newBeatString2, out int newBeatInt2) ||
                !int.TryParse(newBeatString3, out int newBeatInt3))
            {
                selectedBpmItem.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(newBeatInt1, newBeatInt2, newBeatInt3, out Beat newBeat))
            {
                selectedBpmItem.ForceNotify();
                return;
            }

            if (selectedBpmItem.CurrentValue.StartBeat == newBeat) // 按字段相等性比较，即 [0,1,2] != [0,2,4]，即使他们在计算上是相等的
                return;

            // 校验 newBeat 在整个 bpmList 中的合法性
            List<BpmGroupItem> newList = new List<BpmGroupItem>(Model.ChartPackData.CurrentValue.BpmGroup);
            newList.Remove(SelectedBpmItem.CurrentValue);
            newList.Add(new BpmGroupItem(selectedBpmItem.CurrentValue.Bpm, newBeat));
            if (BpmGroupHelper.Validate(newList) == BpmGroupHelper.BpmValidationStatus.Invalid)
            {
                // 强制纠正 UI 的详情面板属性
                selectedBpmItem.ForceNotify();
                return;
            }

            var oldBeat = selectedBpmItem.CurrentValue.StartBeat;
            CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        selectedBpmItem.CurrentValue.StartBeat = newBeat;
                        BpmGroupHelper.Sort(Model.ChartPackData.CurrentValue.BpmGroup);
                        Model.BpmGroupDataChangedSubject.OnNext(itemIndex);
                        selectedBpmItem.ForceNotify(); // 刷新当前选中的 item 的详情面板属性，如 Number
                    },
                    () =>
                    {
                        selectedBpmItem.CurrentValue.StartBeat = oldBeat;
                        BpmGroupHelper.Sort(Model.ChartPackData.CurrentValue.BpmGroup);
                        Model.BpmGroupDataChangedSubject.OnNext(itemIndex);
                        selectedBpmItem.ForceNotify(); // 刷新当前选中的 item 的详情面板属性，如 Number
                    }
                )
            );
        }

        /// <summary>
        /// 删除当前选定的 bpmItem
        /// </summary>
        public void DeleteSelectedBpmItem()
        {
            if (selectedBpmItem.CurrentValue == null)
                throw new Exception("未选中 BpmItem");

            if (Model.ChartPackData.CurrentValue.BpmGroup.IndexOf(SelectedBpmItem.CurrentValue) == 0)
                throw new Exception("不允许删除首个 BpmItem");

            var oldBpmItem = selectedBpmItem.CurrentValue;
            var oldIndex = Model.ChartPackData.CurrentValue.BpmGroup.IndexOf(oldBpmItem);

            CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.BpmGroup.RemoveAt(oldIndex);
                        selectedBpmItem.Value = null;
                    },
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.BpmGroup.Insert(oldIndex, oldBpmItem);
                        selectedBpmItem.Value = oldBpmItem;
                    }
                )
            );
        }
    }
}
