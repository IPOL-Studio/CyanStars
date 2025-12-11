/// <summary>
/// 用于内置谱包的定数结构体
/// </summary>
[System.Serializable]
public struct ChartPackLevels
{
    public float KuiXingLevel;
    public float QiMingLevel;
    public float TianShuLevel;
    public float WuYinLevel;

    public ChartPackLevels(float kuiXingLevel = 0, float qiMingLevel = 0, float tianShuLevel = 0, float wuYinLevel = 0)
    {
        KuiXingLevel = kuiXingLevel;
        QiMingLevel = qiMingLevel;
        TianShuLevel = tianShuLevel;
        WuYinLevel = wuYinLevel;
    }
}
