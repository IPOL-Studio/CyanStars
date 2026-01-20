#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Utils;
using UnityEngine;

public class ChartEditorSceneRoot : MonoBehaviour
{
    [SerializeField]
    private MvvmBindManager mvvmBindManager = null!;

    [SerializeField]
    private CommandManager commandManager = null!;

    [SerializeField]
    private ChartEditorMusicManager musicManager = null!;

    [SerializeField]
    private ChartEditorFileManager fileManager = null!;


    public static MvvmBindManager MvvmBindManager = null!;
    public static CommandManager CommandManager = null!;
    public static ChartEditorMusicManager MusicManager = null!;
    public static ChartEditorFileManager FileManager = null!;


    private void Awake()
    {
        MvvmBindManager = mvvmBindManager;
        CommandManager = commandManager;
        MusicManager = musicManager;
        FileManager = fileManager;
    }


    private void Start()
    {
        var chartModule = GameRoot.GetDataModule<ChartModule>();

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
            ChartMetaData chartMetaData = new ChartMetaData(randomName);
            List<BpmGroupItem> bpmGroup = new List<BpmGroupItem>();
            _ = Beat.TryCreateBeat(0, 0, 1, out Beat beat);
            bpmGroup.Add(new BpmGroupItem(128, beat));
            chartPackData = new ChartPackData(randomName, bpmGroup: bpmGroup, chartMetaDatas: new List<ChartMetaData> { chartMetaData });

            chartMetadataIndex = 0;
        }
        else if (chartModule.ChartData is null)
        {
            // 打开谱包，但创建新谱面
            workspacePath = chartModule.SelectedRuntimeChartPack.WorkspacePath;
            throw new NotImplementedException(); //TODO
        }
        else
        {
            // 打开谱包和谱面
            workspacePath = chartModule.SelectedRuntimeChartPack.WorkspacePath;
            throw new NotImplementedException(); //TODO
        }

        mvvmBindManager.StartBind(workspacePath, chartMetadataIndex, chartPackData, chartData, commandManager, musicManager);
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
