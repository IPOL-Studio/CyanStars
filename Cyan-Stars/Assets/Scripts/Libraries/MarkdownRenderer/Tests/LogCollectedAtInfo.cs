using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace CyanStars.MarkdownRenderer.Tests
{
    public class LogCollectedAtInfo : MonoBehaviour
    {
        public void Print(IReadOnlyList<TextMeshProMarkdownAtInfoCollector.AtInfo> atInfos)
        {
            Debug.Log(string.Join("\n", atInfos.Select(info => $"{info.Content} : {info.Link}")));
        }
    }
}
