using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// 路径类型
    /// </summary>
    public enum PathType
    {
        /// <summary>
        /// Unkown
        /// </summary>
        Unkown,
        /// <summary>
        /// Full path
        /// </summary>
        FullPath,
        /// <summary>
        /// Relative path
        /// </summary>
        RelativePath,
        /// <summary>
        /// web url
        /// </summary>
        Url,
    }

    /// <summary>
    /// Path的类型判断
    /// </summary>
    public static class CheckPath
    {
        /// <summary>
        /// Path的类型判断
        /// </summary>
        /// <param name="path">Path的值</param>
        /// <returns></returns>
        public static PathType CheckPathType(string path)
        {
            // 判断路径开头
            var tmpStr = path.Split(new char[] {':'}, 2);
            var head = tmpStr[0];

            // url
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return PathType.Url;
            }

            // 非url
            if (head.Length == 1)
            {
                if (Char.IsLetter(head[0]))
                {
                    return PathType.FullPath;
                }
            }
            else
            {
                if (path.StartsWith(".\\") || path.StartsWith("..\\") || path.StartsWith("./") || path.StartsWith("../"))
                {
                    return PathType.RelativePath;
                }
            }

            return PathType.Unkown;
        }
    }
}
