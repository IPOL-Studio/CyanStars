#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.Management
{
    /// <summary>
    /// 定义制谱器内 PlayerPrefs 的数据结构并管理其加载和读取
    /// </summary>
    public class ChartEditorPlayerPrefsManager : MonoBehaviour
    {
        // 注意：谨慎修改此处 string 值，可能导致谱师丢失之前版本的 PlayerPrefs
        private const string PosAccuracyPrefName = "ChartEditor_PosAccuracy";
        private const string PosMagnetPrefName = "ChartEditor_PosMagnet";
        private const string BeatAccuracyPrefName = "ChartEditor_BeatAccuracy";
        private const string BeatZoomPrefName = "ChartEditor_BeatZoom";

        private const string IsCompactNoteButtonAreaName = "ChartEditor_IsCompactNoteButtonArea";
        private const string IsMultiBpmItemName = "ChartEditor_IsMultiBpmItem";
        private const string IsMultiMusicItemName = "ChartEditor_IsMultiMusicItem";

        private readonly CompositeDisposable Disposables = new CompositeDisposable();

        /// <summary>
        /// 初始化并绑定 Model，实现数据的读取与自动保存
        /// </summary>
        public void Init(ChartEditorModel model)
        {
            InitChartEditorPlayerPrefs(model);
            InitChartEditorSettingPlayerPrefs(model);
        }

        private void InitChartEditorPlayerPrefs(ChartEditorModel model)
        {
            if (PlayerPrefs.HasKey(PosAccuracyPrefName))
                model.PosAccuracy.Value = PlayerPrefs.GetInt(PosAccuracyPrefName);
            if (PlayerPrefs.HasKey(PosMagnetPrefName))
                // PlayerPrefs 不支持 bool，用 1 表示 true，0 表示 false
                model.PosMagnet.Value = PlayerPrefs.GetInt(PosMagnetPrefName) == 1;
            if (PlayerPrefs.HasKey(BeatAccuracyPrefName))
                model.BeatAccuracy.Value = PlayerPrefs.GetInt(BeatAccuracyPrefName);
            if (PlayerPrefs.HasKey(BeatZoomPrefName))
                // PlayerPrefs 不支持 double，以 string 存储
                if (double.TryParse(PlayerPrefs.GetString(BeatZoomPrefName), out var zoomVal))
                    model.BeatZoom.Value = zoomVal;

            // 使用 Skip(1)，跳过首次订阅引起的初始化操作
            model.PosAccuracy
                .Skip(1)
                .Subscribe(val =>
                {
                    PlayerPrefs.SetInt(PosAccuracyPrefName, val);
                    PlayerPrefs.Save();
                })
                .AddTo(Disposables);
            model.PosMagnet
                .Skip(1)
                .Subscribe(val =>
                {
                    PlayerPrefs.SetInt(PosMagnetPrefName, val ? 1 : 0);
                    PlayerPrefs.Save();
                })
                .AddTo(Disposables);
            model.BeatAccuracy
                .Skip(1)
                .Subscribe(val =>
                {
                    PlayerPrefs.SetInt(BeatAccuracyPrefName, val);
                    PlayerPrefs.Save();
                })
                .AddTo(Disposables);
            model.BeatZoom
                .Skip(1)
                .Subscribe(val =>
                {
                    PlayerPrefs.SetString(BeatZoomPrefName, val.ToString("G")); // "G" 保证常规精度
                    PlayerPrefs.Save();
                })
                .AddTo(Disposables);
        }

        private void InitChartEditorSettingPlayerPrefs(ChartEditorModel model)
        {
            if (PlayerPrefs.HasKey(IsCompactNoteButtonAreaName))
                model.IsCompactNoteButtonArea.Value = PlayerPrefs.GetInt(IsCompactNoteButtonAreaName) == 1;
            if (PlayerPrefs.HasKey(IsMultiBpmItemName))
                model.IsMultiBpmItemMode.Value = PlayerPrefs.GetInt(IsMultiBpmItemName) == 1;
            if (PlayerPrefs.HasKey(IsMultiMusicItemName))
                model.IsMultiMusicItemMode.Value = PlayerPrefs.GetInt(IsMultiMusicItemName) == 1;

            model.IsCompactNoteButtonArea
                .Skip(1)
                .Subscribe(val =>
                {
                    PlayerPrefs.SetInt(IsCompactNoteButtonAreaName, val ? 1 : 0);
                    PlayerPrefs.Save();
                })
                .AddTo(Disposables);
            model.IsMultiBpmItemMode
                .Skip(1)
                .Subscribe(val =>
                {
                    PlayerPrefs.SetInt(IsMultiBpmItemName, val ? 1 : 0);
                    PlayerPrefs.Save();
                })
                .AddTo(Disposables);
            model.IsMultiMusicItemMode
                .Skip(1)
                .Subscribe(val =>
                {
                    PlayerPrefs.SetInt(IsMultiMusicItemName, val ? 1 : 0);
                    PlayerPrefs.Save();
                })
                .AddTo(Disposables);
        }

        private void OnDestroy()
        {
            Disposables.Dispose();
        }
    }
}
