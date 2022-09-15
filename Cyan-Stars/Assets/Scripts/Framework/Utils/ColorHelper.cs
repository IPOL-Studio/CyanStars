using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CyanStars.Framework.Utils
{
    public static class ColorHelper
    {
        private static readonly Dictionary<string, string> ColorNameToHex = new Dictionary<string, string>
        {
            {"aqua", "#00FFFF"},
            {"black", "#000000"},
            {"blue", "#0000FF"},
            {"brown", "#A52A2A"},
            {"cyan", "#00FFFF"},
            {"green", "#008000"},
            {"grey", "#808080"},
            {"orange", "#FFA500"},
            {"purple", "#800080"},
            {"red", "#FF0000"},
            {"white", "#FFFFFF"}
        };

        public static bool TryParseColor(string colorName, out string hex, bool isToLower = true)
        {
            if (isToLower)
                colorName = colorName.ToLower();
            return ColorNameToHex.TryGetValue(colorName, out hex);
        }

        public static bool TryParseHtmlString(string str, out string hex)
        {
            hex = string.Empty;
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            if (str[0] != '#')
            {
                return TryParseColor(str, out hex);
            }

            if (!IsHexColor(str)) return false;

            if (str.Length == 4)
            {
                // 将 RGB 扩展为 RRGGBB
                // 避免 TMP 部分标签不认 #RGB 的问题

                // 在升级到支持 .net standard2.1 的 Unity 后
                // 检查这里的 unsafe 并将 pointer 切换到 Span
                unsafe
                {
                    var buffer = stackalloc char[7];
                    buffer[0] = '#';
                    char c = str[1];
                    for (int i = 1; i < 7; i++)
                    {
                        buffer[i] = c;
                    }
                    hex = new string(buffer, 0, 7);
                }
            }
            else
            {
                hex = str;
            }
            return true;

        }

        public static bool IsHexColor(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str) || str[0] != '#')
            {
                return false;
            }

            switch (str.Length)
            {
                case 4:  //#RGB
                    return IsHexColorChar(str[1]) && str[2] == str[1] && str[3] == str[1];
                case 7:  //#RRGGBB
                case 9:  //#RRGGBBAA
                {
                    for (int i = 1; i < str.Length; i++)
                    {
                        var c = str[i];
                        if (!IsHexColorChar(c))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsHexColorChar(char c)
        {
            return char.IsDigit(c) || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';
        }
    }
}
