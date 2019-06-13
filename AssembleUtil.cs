using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// Assemble工具
    /// </summary>
    public static class AssembleUtil
    {
        public static T SetPorpertyValues<T>(Dictionary<string, object> values, BindingFlags flags)
        {
            T obj = Activator.CreateInstance<T>();
            return SetPorpertyValues(values, flags, obj);
        }

        public static T SetPorpertyValues<T>(Dictionary<string, object> values, BindingFlags flags, T obj)
        {
            var properties = GetPorperties<T>(flags);
            foreach (var p in properties)
            {
                if (values.ContainsKey(p.Key))
                {
                    p.Value.SetValue(obj, values[p.Key], new object[] { });
                }
            }
            return obj;
        }

        public static Dictionary<string, object> GetPropertyValues(object obj, BindingFlags flags)
        {
            var result = new Dictionary<string, object>();
            try
            {
                var properties = obj.GetType().GetProperties(flags);
                foreach (var pro in properties)
                {
                    result.Add(pro.Name, pro.GetValue(obj, new object[] { }));
                }
            }
            catch (Exception e)
            {
                StaticLogger.Instance.Error("Failed to get the property values for object, exception: {0}", e);
            }
            return result;
        }

        public static List<string> GetPorpertyNames<T>(BindingFlags flags)
        {
            var result = new List<string>();
            try
            {
                var properties = typeof(T).GetProperties(flags);
                foreach(var p in properties)
                {
                    result.Add(p.Name);
                }
            }
            catch (Exception e)
            {
                StaticLogger.Instance.Error("Failed to get the properties for object, exception: {0}", e);
            }
            return result;
        }

        private static Dictionary<string, PropertyInfo> GetPorperties<T>(BindingFlags flags)
        {
            var result = new Dictionary<string, PropertyInfo>();
            try
            {
                var properties = typeof(T).GetProperties(flags);
                foreach (var p in properties)
                {
                    result[p.Name] = p;
                }
            }
            catch (Exception e)
            {
                StaticLogger.Instance.Error("Failed to get the properties for object, exception: {0}", e);
            }
            return result;
        }
    }
}
