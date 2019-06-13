using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    public class CsvHandler
    {
        private static readonly System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance;
        public static void WriteToFile<T>(string path, IEnumerable<T> datas)
        {
            path = path.EndsWith(".csv") ? path : path + ".csv";
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.WriteLine(GetHeaderString<T>());
                foreach(var d in datas)
                {
                    sw.WriteLine(GetValueString(d));
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
                    var tempValues = line.Split(',');
                    Dictionary<string, object> values = new Dictionary<string, object>();
                    for(int i = 0; i < props.Count; ++i)
                    {
                        values[props[i]] = tempValues[i].Trim('\"');
                    }
                    result.Add(SetValueToObj<T>(values));
                }
            }
            return result;
        }

        private static T SetValueToObj<T>(Dictionary<string, object> values)
        {
            return AssembleUtil.SetPorpertyValues<T>(values, flags);
        }

        private static List<string> GetPropertyNames(string headerLine)
        {
            return new List<string>(headerLine.Split(',').Select(h => { return h.Trim('\"'); }));
        }

        private static string GetHeaderString<T>()
        {
            StringBuilder header = new StringBuilder();
            var props = AssembleUtil.GetPorpertyNames<T>(flags);
            foreach(var p in props)
            {
                header.AppendFormat("{0},", p);
            }
            return header.ToString().TrimEnd(',');
        }

        private static string GetValueString<T>(T data)
        {
            StringBuilder value = new StringBuilder();
            var objValues = AssembleUtil.GetPropertyValues(data, flags);
            foreach (var v in objValues.Values)
            {
                var temp = v.ToString();
                if(temp.Contains(",") || temp.Contains(" "))
                {
                    temp = string.Format("\"{0}\"", v);
                }
                value.AppendFormat("{0},", temp);
            }
            return value.ToString().TrimEnd(',');
        }
    }
}
