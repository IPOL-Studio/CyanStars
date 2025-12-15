using CyanStars.Chart;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    public class ChartEditorModel
    {
        // 运行时数据，在实例化 Model 时固定
        private readonly string WorkspacePath; // 当前的工作区绝对路径（谱包索引文件所在路径）
        private readonly ChartMetadata ChartMetadata; // 当前编辑的谱面对应的谱包中的元数据

        // 当前正在编辑的谱包和谱面内容
        public ChartPackData ChartPackData;
        public ChartData ChartData;


        // 构造函数
        public ChartEditorModel(string workspacePath,
                                ChartMetadata chartMetadata,
                                ChartPackData chartPackData,
                                ChartData chartData)
        {
            WorkspacePath = workspacePath;
            ChartMetadata = chartMetadata;

            ChartPackData = chartPackData;
            ChartData = chartData;
        }
    }
}
