#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyanStars.Chart
{
    public interface ISpeedTemplateProvider
    {
        public SpeedTemplate GetSpeedTemplate(SpeedTemplateData data);
    }

    internal sealed class SpeedTemplateProvider : ISpeedTemplateProvider
    {
        private readonly float PlayerSpeed;

        private readonly ISpeedTemplateBaker Baker;
        private Dictionary<SpeedTemplateData, SpeedTemplate>? cache;

        public SpeedTemplateProvider(ISpeedTemplateBaker baker, float playerSpeed)
        {
            Baker = baker ?? throw new ArgumentNullException(nameof(baker));
            PlayerSpeed = playerSpeed;
        }

        public SpeedTemplate GetSpeedTemplate(SpeedTemplateData data)
        {
            if (cache is null)
            {
                throw new InvalidOperationException("SpeedTemplateProvider cache is not populated yet.");
            }

            return cache[data];
        }

        public void PopulateSpeedTemplates(IReadOnlyCollection<SpeedTemplateData> dataList)
        {
            cache = Baker.IsSupportParallel
                ? dataList.AsParallel()
                          .ToDictionary(data => data, data => SpeedTemplate.Create(data, Baker, PlayerSpeed))
                : dataList.ToDictionary(data => data, data => SpeedTemplate.Create(data, Baker, PlayerSpeed));
        }
    }
}
