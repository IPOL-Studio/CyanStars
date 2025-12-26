#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    public class ChartEditorModel
    {
        // == == 谱包和谱面数据 == ==

        // 元数据，在实例化 Model 时固定
        public readonly string WorkspacePath; // 当前的工作区绝对路径（谱包索引文件所在路径）
        public readonly int ChartMetaDataIndex; // 当前编辑的谱面对应的谱包中的元数据

        // 当前正在编辑的谱包和谱面内容
        public readonly ReadOnlyReactiveProperty<ChartPackDataEditorModel> ChartPackData;
        public readonly ReadOnlyReactiveProperty<ChartDataEditorModel> ChartData;


        // == == 编辑器运行时数据 == ==
        public readonly ReactiveProperty<bool> IsSimplificationMode = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<EditToolType> SelectedEditTool = new ReactiveProperty<EditToolType>(EditToolType.Select);

        public readonly ReactiveProperty<bool> ChartPackDataCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> ChartDataCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> MusicVersionCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> BpmGroupCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> SpeedTemplateCanvasVisibility = new ReactiveProperty<bool>(false); // TODO

        public readonly ReactiveProperty<int> PosAccuracy = new ReactiveProperty<int>(4);
        public readonly ReactiveProperty<bool> PosMagnet = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<int> BeatAccuracy = new ReactiveProperty<int>(2);
        public readonly ReactiveProperty<float> BeatZoom = new ReactiveProperty<float>(1f);


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="workspacePath">工作区绝对路径（谱包索引文件所在路径）</param>
        /// <param name="chartMetaDataIndex">谱面在谱包元数据中的索引</param>
        /// <param name="chartPackData">要修改的谱包数据，注意请先深拷贝一份</param>
        /// <param name="chartData">要修改的谱面数据，注意请先深拷贝一份</param>
        /// <param name="commandManager">命令管理器实例</param>
        public ChartEditorModel(string workspacePath,
                                int chartMetaDataIndex,
                                ChartPackData chartPackData,
                                ChartData chartData)
        {
            WorkspacePath = workspacePath;
            ChartMetaDataIndex = chartMetaDataIndex;

            ChartPackData = new ReactiveProperty<ChartPackDataEditorModel>(new ChartPackDataEditorModel(chartPackData));
            ChartData = new ReactiveProperty<ChartDataEditorModel>(new ChartDataEditorModel(chartData));
        }
    }
}
