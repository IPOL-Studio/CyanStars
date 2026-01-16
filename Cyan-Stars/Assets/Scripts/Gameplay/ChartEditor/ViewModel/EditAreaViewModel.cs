#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class EditAreaViewModel : BaseViewModel
    {
        private const float DefaultMajorBeatLineInterval = 250f;

        public readonly ReadOnlyReactiveProperty<int> BeatAccuracy;
        public readonly ReadOnlyReactiveProperty<float> BeatZoom;

        public readonly ReadOnlyReactiveProperty<float> ContentHeight;
        public readonly ReadOnlyReactiveProperty<float> TotalBeats;
        public readonly ReadOnlyReactiveProperty<int> PosLineCount;


        public EditAreaViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            BeatAccuracy = Model.BeatAccuracy.ToReadOnlyReactiveProperty();
            BeatZoom = Model.BeatZoom.ToReadOnlyReactiveProperty();

            // 更新 ContentHeight，当：
            // - BpmGroup 列表更新（元素增加、删除、移动）
            // - 手动触发了 BpmGroupDataChangedSubject（BpmGroup 中某一元素的 bpm 或 startBeat 更新）
            // - BeatZoom 更新
            // - AudioClipHandler 更新（音乐变化后音频时长可能改变）
            ContentHeight = Observable
                .Merge(
                    Model.ChartPackData.CurrentValue.BpmGroup.ObserveChanged().Select(_ => Unit.Default),
                    Model.BpmGroupDataChangedSubject.Select(_ => Unit.Default)
                )
                .Prepend(Unit.Default)
                .CombineLatest(
                    Model.BeatZoom,
                    Model.AudioClipHandler,
                    (_, zoom, handler) =>
                    {
                        if (handler?.Asset is null)
                            return 0f;

                        var bpmGroup = Model.ChartPackData.CurrentValue.BpmGroup;
                        if (BpmGroupHelper.Validate(bpmGroup) != BpmGroupHelper.BpmValidationStatus.Valid)
                            throw new Exception("Bpm 组数据异常");

                        float beatCount =
                            BpmGroupHelper.CalculateBeat(bpmGroup, (int)(handler.Asset.length * 1000f));
                        return beatCount * DefaultMajorBeatLineInterval * zoom;
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            // 更新 TotalBeats，当：
            // - firstMusicVersionItemOffset 发生了更新（MusicVersion 首个元素的索引发生更新，或首个元素内的 offset 属性更新）
            // - BpmGroup 列表更新（元素增加、删除、移动）
            // - 手动触发了 BpmGroupDataChangedSubject（BpmGroup 中某一元素的 bpm 或 startBeat 更新）
            // - AudioClipHandler 更新（音乐变化后音频时长可能改变）
            // （bro，这下面的代码可太炸裂了...）
            ReadOnlyReactiveProperty<int?> firstMusicVersionItemOffset = // 始终监听到第一个 MusicVersionItem 的 offset 变化
                Model.ChartPackData.CurrentValue.MusicVersions
                    .ObserveChanged()
                    .Prepend((CollectionChangedEvent<MusicVersionDataEditorModel>)default)
                    .Select(_ =>
                        Model.ChartPackData.CurrentValue.MusicVersions.Count > 0
                            ? Model.ChartPackData.CurrentValue.MusicVersions[0]
                            : null
                    )
                    .DistinctUntilChanged()
                    .Select(data => data != null
                        ? data.Offset.AsObservable().Select(x => (int?)x)
                        : Observable.Return((int?)null)
                    )
                    .Switch()
                    .ToReadOnlyReactiveProperty(null)
                    .AddTo(base.Disposables);
            TotalBeats = Observable
                .Merge(
                    Model.ChartPackData.CurrentValue.BpmGroup.ObserveChanged().Select(_ => Unit.Default),
                    Model.BpmGroupDataChangedSubject.Select(_ => Unit.Default)
                )
                .Prepend(Unit.Default)
                .CombineLatest(
                    firstMusicVersionItemOffset,
                    Model.AudioClipHandler,
                    (_, offset, handler) =>
                    {
                        if (offset == null || handler?.Asset is null)
                            return 0;

                        var bpmGroup = Model.ChartPackData.CurrentValue.BpmGroup;
                        int totalTimelineTimeMs = (int)(handler.Asset.length * 1000) + (int)offset;
                        return BpmGroupHelper.CalculateBeat(bpmGroup, totalTimelineTimeMs);
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);


            PosLineCount = Model.PosAccuracy
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<int>.Instance, 4)
                .AddTo(base.Disposables);
        }

        public float GetBeatLineDistance()
        {
            return DefaultMajorBeatLineInterval * BeatZoom.CurrentValue / BeatAccuracy.CurrentValue;
        }
    }
}
