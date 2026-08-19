using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CyanStars.MarkdownRenderer.Utils;
namespace CyanStars.MarkdownRenderer.Tests
{
    public class LogCollectedAtInfo : MonoBehaviour
    {
        public void Print(IReadOnlyList<AtInfo> atInfos)
        {
            Debug.Log(string.Join("\n", atInfos.Select(info => $"{info.Content} : {info.Link}")));
        }
    }
}
