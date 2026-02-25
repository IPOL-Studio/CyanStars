#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CyanStars.Chart
{
    public interface ISpeedTemplateBaker
    {
        bool IsSupportParallel { get; }

        /// <summary>
        /// 烘焙 速度-时间 和 位移-时间 列表
        /// <returns>如果曲线组贝塞尔点小于等于 1 个，返回 false。请直接用 [^1].Value 获取</returns>
        /// </summary>
        bool Bake(SpeedTemplateData speedTemplateData,
                  float playerSpeed,
                  [NotNullWhen(true)] out List<float>? speedList,
                  [NotNullWhen(true)] out List<float>? displacementList);

        /// <summary>
        /// 获取整组曲线结束时的最终位移
        /// </summary>
        double GetFinalDisplacement(SpeedTemplateData speedTemplateData, float playerSpeed);
    }
}
