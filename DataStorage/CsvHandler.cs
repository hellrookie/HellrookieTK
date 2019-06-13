using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hellrookie.ToolKit
{
    public class CsvHandler
    {
        public static void WriteToFile<T>(string path, IEnumerable<T> datas)
        {
            path = path.EndsWith(".csv") ? path : path + ".csv";
            var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.WriteLine(GetHeaderString(props));
                foreach(var d in datas)
                {
                    sw.WriteLine(GetValueString(props, d));
                }
            }
        }

        public static List<T> ReadFromFile<T>(string path)
        {
            var result = new List<T>();
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                var props = GetPropertyNames(sr.ReadLine());
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var values = line.Split(',');
                    result.Add(SetValueToObj<T>(props, values.Select(v => { return v.Trim('\"'); }).ToList()));
                }
            }
            return result;
        }

        private static T SetValueToObj<T>(List<string> propNames, List<string> values)
        {
            T obj = Activator.CreateInstance<T>();
            Dictionary<string, PropertyInfo> objPorps = (typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty)).ToDictionary(p => p.Name, p=>p);
            for(int i = 0; i < propNames.Count(); ++i)
            {
                if(objPorps.ContainsKey(propNames[i]))
                {
                    objPorps[propNames[i]].SetValue(obj, values[i], null);
                }
            }
            return obj;
        }

        private static List<string> GetPropertyNames(string headerLine)
        {
            return new List<string>(headerLine.Split(',').Select(h => { return h.Trim('\"'); }));
        }

        private static string GetHeaderString(IEnumerable<PropertyInfo> props)
        {
            StringBuilder header = new StringBuilder();
            foreach(var p in props)
            {
                header.AppendFormat("{0},", p.Name);
            }
            return header.ToString().TrimEnd(',');
        }

        private static string GetValueString<T>(IEnumerable<PropertyInfo> props, T data)
        {
            StringBuilder value = new StringBuilder();
            foreach (var p in props)
            {
                var tempValue = p.GetValue(data, null).ToString();
                if(tempValue.Contains(","))
                {
                    tempValue = string.Format("\"{0}\"", tempValue);
                }
                value.AppendFormat("{0},", tempValue);
            }
            return value.ToString().TrimEnd(',');
        }
    }
}
