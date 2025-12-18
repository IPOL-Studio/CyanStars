#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.BindableProperty;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    public class ChartEditorModel
    {
        // 元数据，在实例化 Model 时固定
        private readonly string WorkspacePath; // 当前的工作区绝对路径（谱包索引文件所在路径）
        private readonly ChartMetadata ChartMetadata; // 当前编辑的谱面对应的谱包中的元数据

        // 当前正在编辑的谱包和谱面内容
        public BindableProperty<ChartPackData> ChartPackData;
        public BindableProperty<ChartData> ChartData;

        // 运行时数据
        public readonly BindableProperty<EditToolType> SelectedEditTool = new BindableProperty<EditToolType>(EditToolType.Select);

        public readonly BindableProperty<bool> ChartPackDataCanvasVisibility = new BindableProperty<bool>(false);
        public readonly BindableProperty<bool> ChartDataCanvasVisibility = new BindableProperty<bool>(false);
        public readonly BindableProperty<bool> MusicVersionCanvasVisibility = new BindableProperty<bool>(false);
        public readonly BindableProperty<bool> BpmGroupCanvasVisibility = new BindableProperty<bool>(false);
        public readonly BindableProperty<bool> SpeedTemplateCanvasVisibility = new BindableProperty<bool>(false); // TODO

        public readonly BindableProperty<int> PosAccuracy = new BindableProperty<int>(4);
        public readonly BindableProperty<bool> PosMagnet = new BindableProperty<bool>(true);
        public readonly BindableProperty<int> BeatAccuracy = new BindableProperty<int>(2);
        public readonly BindableProperty<float> BeatZoom = new BindableProperty<float>(1f);


        // 构造函数
        public ChartEditorModel(string workspacePath,
                                ChartMetadata chartMetadata,
                                ChartPackData chartPackData,
                                ChartData chartData)
        {
            WorkspacePath = workspacePath;
            ChartMetadata = chartMetadata;

            ChartPackData = new BindableProperty<ChartPackData>(chartPackData);
            ChartData = new BindableProperty<ChartData>(chartData);
        }
    }
}
