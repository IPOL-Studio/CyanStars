#nullable enable

using System;
using Newtonsoft.Json;

namespace CyanStars.Chart
{
    public class ChartPackLinkData
    {
        /// <summary>
        /// Icon 图标
        /// </summary>
        public LinkIcon? LinkIcon;

        /// <summary>
        /// 按钮标题文本
        /// </summary>
        public string LinkTitle;

        /// <summary>
        /// 要跳转的 URL
        /// </summary>
        public string LinkUrl;


        /// <summary>
        /// 构造函数
        /// </summary>
        [JsonConstructor]
        public ChartPackLinkData(LinkIcon? linkIcon, string linkTitle, string linkUrl)
        {
            LinkIcon = linkIcon;
            LinkTitle = linkTitle;
            LinkUrl = linkUrl;
        }
    }

    public enum LinkIcon
    {
        GitHub,
        Gitee,
        NeteaseMusic
    }

    public static class LinkIconExtensions
    {
        public static string GetIconPath(this LinkIcon icon) => icon switch
        {
            LinkIcon.GitHub => "Assets/CysMultimediaAssets/Sprites/SimpleIcons/github.png",
            LinkIcon.Gitee => "Assets/CysMultimediaAssets/Sprites/SimpleIcons/gitee.png",
            LinkIcon.NeteaseMusic => "Assets/CysMultimediaAssets/Sprites/SimpleIcons/neteasecloudmusic.png",
            _ => throw new ArgumentOutOfRangeException(nameof(icon), icon, null)
        };
    }
}
