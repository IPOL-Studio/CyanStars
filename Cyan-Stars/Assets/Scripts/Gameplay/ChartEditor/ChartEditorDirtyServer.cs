#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Management
{
    /// <summary>
    /// 制谱器"是否存在未保存数据"的服务
    /// </summary>
    public class ChartEditorDirtyServer : IDisposable
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();
        private readonly ReactiveProperty<bool> hasUnsavedChanges = new ReactiveProperty<bool>(false);

        /// <summary>
        /// 是否存在未保存数据
        /// </summary>
        public ReadOnlyReactiveProperty<bool> HasUnsavedChanges => hasUnsavedChanges;


        /// <summary>
        /// 绑定到制谱器 Model
        /// </summary>
        /// <param name="model">制谱器 Model</param>
        /// <param name="initialHasUnsavedChanges">会话初始是否存在未保存数据（新建谱面为 true，加载已有谱面为 false）</param>
        public ChartEditorDirtyServer(ChartEditorModel model, bool initialHasUnsavedChanges)
        {
            hasUnsavedChanges.Value = initialHasUnsavedChanges;

            GetDataChangeSource(model)
                .Subscribe(_ => hasUnsavedChanges.Value = true)
                .AddTo(Disposables);
        }

        /// <summary>
        /// 标记为已保存（保存成功后调用）
        /// </summary>
        public void MarkSaved() => hasUnsavedChanges.Value = false;

        public void Dispose() => Disposables.Dispose();


        private static Observable<Unit> GetDataChangeSource(ChartEditorModel model)
        {
            var cp = model.ChartPackData.CurrentValue;
            var cd = model.ChartData.CurrentValue;

            return Observable.Merge(
                cd.ReadyBeat.AsObservable().Skip(1).Select(_ => Unit.Default),
                ToUnit(cd.SpeedGroupDatas),
                ToUnit(cd.Notes),
                ToUnit(cd.TrackDatas),
                cp.DataVersion.AsObservable().Skip(1).Select(_ => Unit.Default),
                cp.Title.AsObservable().Skip(1).Select(_ => Unit.Default),
                cp.ChartPackInfo.AsObservable().Skip(1).Select(_ => Unit.Default),
                cp.MusicPreviewStartBeat.AsObservable().Skip(1).Select(_ => Unit.Default),
                cp.MusicPreviewEndBeat.AsObservable().Skip(1).Select(_ => Unit.Default),
                cp.CoverFilePath.AsObservable().Skip(1).Select(_ => Unit.Default),
                cp.CropStartPositionPercent.AsObservable().Skip(1).Select(_ => Unit.Default),
                cp.CropHeightPercent.AsObservable().Skip(1).Select(_ => Unit.Default),
                ToUnit(cp.MusicVersions),
                ToUnit(cp.BpmGroup),
                ToUnit(cp.ChartMetaDatas),

                // 列表子项修改
                model.BpmGroupDataChangedSubject.Select(_ => Unit.Default),
                model.SelectedNoteDataChangedSubject.Select(_ => Unit.Default)
            );
        }

        private static Observable<Unit> ToUnit<T>(ObservableList<T> list) =>
            Observable.Merge(
                list.ObserveAdd().Select(_ => Unit.Default),
                list.ObserveRemove().Select(_ => Unit.Default),
                list.ObserveReplace().Select(_ => Unit.Default),
                list.ObserveReset().Select(_ => Unit.Default)
            );
    }
}
