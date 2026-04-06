#nullable enable

using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Utils;
using Gameplay.ChartEditor;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    public class ChartEditorSceneRoot : MonoBehaviour
    {
        [SerializeField]
        private MvvmBindManager mvvmBindManager = null!;

        [SerializeField]
        private CommandStack commandStack = null!;

        [SerializeField]
        private ChartEditorMusicManager musicManager = null!;

        [SerializeField]
        private ChartEditorNoteAudioManager noteAudioManager = null!;

        [SerializeField]
        private ChartEditorFileManager fileManager = null!;

        [SerializeField]
        private ShortcutManager shortcutManager = null!;


        public static MvvmBindManager MvvmBindManager = null!;
        public static CommandStack CommandStack = null!;
        public static ChartEditorMusicManager MusicManager = null!;
        public static ChartEditorNoteAudioManager NoteAudioManager = null!;
        public static ChartEditorFileManager FileManager = null!;


        private void Awake()
        {
            MvvmBindManager = mvvmBindManager;
            CommandStack = commandStack;
            MusicManager = musicManager;
            FileManager = fileManager;
            NoteAudioManager = noteAudioManager;
        }


        public void InitSceneRoot()
        {
            var chartModule = GameRoot.GetDataModule<ChartModule>();

            // 预热资源防止制谱器播放中动态加载偶发加载失败报错
            // TODO: 这里本来应该用 await 的，但是 Unity Update() 会抢在预热完成前访问 VM，并抛一大堆错误，故先临时凑合，之后重构生命周期管理
            // TODO: 细查动态加载的报错原因
            List<string> assetsToInit = new()
            {
                ChartEditorAssetHelper.PosLinePath,
                ChartEditorAssetHelper.BeatLinePath,
                ChartEditorAssetHelper.TapNotePath,
                ChartEditorAssetHelper.HoldNotePath,
                ChartEditorAssetHelper.DragNotePath,
                ChartEditorAssetHelper.ClickNotePath,
                ChartEditorAssetHelper.BreakNotePath
            };
            _ = GameRoot.Asset.BatchLoadAssetAsync(assetsToInit).BindTo(gameObject);

            string workspacePath;
            int chartMetadataIndex;
            ChartPackData chartPackData;
            ChartData chartData;

            if (chartModule.SelectedRuntimeChartPack is null)
            {
                // 创建新谱包和谱面
                string randomName = CreateRandomName();

                workspacePath = PathUtil.Combine(chartModule.PlayerChartPacksFolderPath, randomName);

                chartData = new ChartData();

                ChartMetaData chartMetaData = new ChartMetaData($"Charts/{randomName}.json");
                List<BpmGroupItem> bpmGroup = new List<BpmGroupItem>();
                _ = Beat.TryCreateBeat(0, 0, 1, out Beat beat);
                bpmGroup.Add(new BpmGroupItem(128, beat));
                chartPackData = new ChartPackData(randomName, bpmGroup: bpmGroup, chartMetaDatas: new List<ChartMetaData> { chartMetaData });

                chartMetadataIndex = 0;
            }
            else if (chartModule.ChartData is null)
            {
                // 打开谱包，但创建新谱面
                string randomName = CreateRandomName();

                workspacePath = chartModule.SelectedRuntimeChartPack.WorkspacePath;

                chartData = new ChartData();

                ChartMetaData chartMetaData = new ChartMetaData($"Charts/{randomName}.json");
                chartPackData = chartModule.SelectedRuntimeChartPack.ChartPackData;
                chartPackData.ChartMetaDatas.Add(chartMetaData);

                chartMetadataIndex = chartPackData.ChartMetaDatas.Count - 1;
            }
            else
            {
                // 打开谱包和谱面
                if (chartModule.SelectedChartMetadataIndex == null)
                    throw new Exception("加载了谱面，但没有正确指定元数据下标");

                workspacePath = chartModule.SelectedRuntimeChartPack.WorkspacePath;
                chartData = chartModule.ChartData;
                chartPackData = chartModule.SelectedRuntimeChartPack.ChartPackData;
                chartMetadataIndex = (int)chartModule.SelectedChartMetadataIndex;
            }

            mvvmBindManager.StartBind(
                workspacePath,
                chartMetadataIndex,
                chartPackData,
                chartData,
                musicManager,
                noteAudioManager,
                shortcutManager
            );
        }

        /// <summary>
        /// 根据用户设备的日期时间和随机 7 位 GUID 拼接字符串
        /// </summary>
        private static string CreateRandomName()
        {
            string timeStr = DateTime.Now.ToString("yyMMddHHmmss");
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 7);
            return $"{timeStr}-{guidPart}";
        }
    }
}
