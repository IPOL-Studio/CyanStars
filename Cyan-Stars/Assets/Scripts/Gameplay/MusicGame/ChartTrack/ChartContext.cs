using System.Collections.Generic;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public record ChartContext(List<BpmGroupItem> BpmGroup, ISpeedTemplateProvider SpeedTemplateProvider);
}
