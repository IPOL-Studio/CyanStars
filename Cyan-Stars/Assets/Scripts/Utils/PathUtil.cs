using System.IO;

namespace Utils
{
    /// <summary>
    /// 提供 Combine 方法，拼接路径并返回全是正斜杠的路径，用于替代 System.IO.Path.Combine()
    /// </summary>
    public class PathUtil
    {
        public static string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2).Replace("\\", "/");
        }

        public static string Combine(string path1, string path2, string path3)
        {
            return Path.Combine(path1, path2, path3).Replace("\\", "/");
        }

        public static string Combine(string path1, string path2, string path3, string path4)
        {
            return Path.Combine(path1, path2, path3, path4).Replace("\\", "/");
        }
    }
}
