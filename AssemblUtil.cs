using System;
using System.Collections.Generic;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// Assemble工具
    /// </summary>
    public static class AssembleUtil
    {
        /// <summary>
        /// 获取对象的Properties(instance|public)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPorperties(object obj)
        {
            var result = new Dictionary<string, string>();
            try
            {
                var properties = obj.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach(var pro in properties)
                {
                    var value = pro.GetValue(obj, new object[] { });
                    var strValue = value == null ? string.Empty : value.ToString();
                    result.Add(pro.Name, strValue);
                }
            }
            catch(Exception e)
            {
                StaticLogger.Instance.Error("Failed to get the properties for object, exception: {0}", e);
            }
            return result;
        }
    }
}
