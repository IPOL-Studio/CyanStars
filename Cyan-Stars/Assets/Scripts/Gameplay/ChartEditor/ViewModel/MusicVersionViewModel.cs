#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MusicVersionViewModel : BaseViewModel
    {
        private const int AddOffsetStep = 10;


        public readonly ObservableList<MusicVersionListItemViewModel> ListItems;
        public readonly ObservableList<MusicVersionStaffItemViewModel> StaffItems; // TODO

        private readonly ReactiveProperty<MusicVersionDataEditorModel?> selectedMusicVersionData;
        public ReadOnlyReactiveProperty<MusicVersionDataEditorModel?> SelectedMusicVersionData => selectedMusicVersionData;

        public readonly ReadOnlyReactiveProperty<bool> CanvasVisibility;
        public readonly ReadOnlyReactiveProperty<bool> ListVisibility;
        public readonly ReadOnlyReactiveProperty<bool> DetailVisibility;

        public readonly ReadOnlyReactiveProperty<string> DetailTitle;
        public readonly ReadOnlyReactiveProperty<string> DetailAudioFilePath;
        public readonly ReadOnlyReactiveProperty<string> DetailOffset;


        public MusicVersionViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            ListItems = new ObservableList<MusicVersionListItemViewModel>();
            foreach (var musicVersionData in Model.ChartPackData.CurrentValue.MusicVersions)
                ListItems.Add(new MusicVersionListItemViewModel(model, commandManager, this, musicVersionData));

            StaffItems = new ObservableList<MusicVersionStaffItemViewModel>();

            selectedMusicVersionData = new ReactiveProperty<MusicVersionDataEditorModel?>(null)
                .AddTo(base.Disposables);

            CanvasVisibility = Model.MusicVersionCanvasVisibility;
            ListVisibility = Observable
                .CombineLatest(
                    model.IsSimplificationMode,
                    model.ChartPackData,
                    (isSimplificationMode, chartPackData) =>
                        isSimplificationMode && chartPackData.MusicVersions.Count > 1
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            DetailVisibility = selectedMusicVersionData
                .Select(data => data != null)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            DetailTitle = SelectedMusicVersionData
                .Select(data => data?.VersionTitle ?? Observable.Return("").AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(SelectedMusicVersionData.CurrentValue?.VersionTitle.Value ?? "")
                .AddTo(base.Disposables);
            DetailAudioFilePath = SelectedMusicVersionData
                .Select(data => data?.AudioFilePath ?? Observable.Return("").AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(SelectedMusicVersionData.CurrentValue?.AudioFilePath.Value ?? "")
                .AddTo(base.Disposables);
            DetailOffset = SelectedMusicVersionData
                .Select(data => data?.Offset ?? Observable.Return(0).AsObservable())
                .Switch()
                .Select(offset => offset.ToString())
                .ToReadOnlyReactiveProperty(SelectedMusicVersionData.CurrentValue?.Offset.Value.ToString() ?? "0")
                .AddTo(base.Disposables);
        }

        /// <summary>
        /// 由子 VM 调用，切换正在编辑的音乐版本数据
        /// </summary>
        /// <param name="musicVersionData">音乐版本数据</param>
        public void SelectEditingMusicVersionData(MusicVersionDataEditorModel? musicVersionData)
        {
            if (selectedMusicVersionData.CurrentValue == musicVersionData)
                return;

            var oldValue = selectedMusicVersionData.CurrentValue;
            CommandManager.ExecuteCommand(new DelegateCommand(
                () => selectedMusicVersionData.Value = musicVersionData,
                () => selectedMusicVersionData.Value = oldValue
            ));
        }


        public void AddMusicVersionItem()
        {
            throw new NotImplementedException();
        }

        public void CloseCanvas()
        {
            if (!CanvasVisibility.CurrentValue)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(
                () => Model.MusicVersionCanvasVisibility.Value = false,
                () => Model.MusicVersionCanvasVisibility.Value = true
            ));
        }

        public void SetTitle(string newTitle)
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置标题。");

            var oldTitle = SelectedMusicVersionData.CurrentValue!.VersionTitle.Value;
            if (oldTitle == newTitle)
                return;
            CommandManager.ExecuteCommand(new DelegateCommand(
                () => SelectedMusicVersionData.CurrentValue!.VersionTitle.Value = newTitle,
                () => SelectedMusicVersionData.CurrentValue!.VersionTitle.Value = oldTitle
            ));
        }

        public void ImportAudioFile()
        {
            throw new NotImplementedException();
        }

        public void MinusOffset()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            CommandManager.ExecuteCommand(new DelegateCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value -= AddOffsetStep,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value += AddOffsetStep
            ));
        }

        public void SetOffset(string text)
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            if (!int.TryParse(text, out int newValue))
            {
                SelectedMusicVersionData.CurrentValue?.Offset.ForceNotify();
                return;
            }

            int oldValue = SelectedMusicVersionData.CurrentValue!.Offset.Value;
            if (oldValue == newValue)
                return;
            CommandManager.ExecuteCommand(new DelegateCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value = newValue,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value = oldValue
            ));
        }

        public void AddOffset()
        {
            if (SelectedMusicVersionData.CurrentValue == null)
                throw new InvalidOperationException("按设计，不允许在没有选中音乐版本数据的情况下设置偏移量。");

            CommandManager.ExecuteCommand(new DelegateCommand(
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value += AddOffsetStep,
                () => SelectedMusicVersionData.CurrentValue!.Offset.Value -= AddOffsetStep
            ));
        }

        public void TestOffset()
        {
            throw new NotImplementedException();
        }

        public void AddStaffItem()
        {
            throw new NotImplementedException();
        }

        public void DeleteItem()
        {
            throw new NotImplementedException();
        }

        public void CloneItem()
        {
            throw new NotImplementedException();
        }

        public void MoveUpItem()
        {
            throw new NotImplementedException();
        }

        public void MoveDownItem()
        {
            throw new NotImplementedException();
        }

        public void TopItem()
        {
            throw new NotImplementedException();
        }
    }
}
