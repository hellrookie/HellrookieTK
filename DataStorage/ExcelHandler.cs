using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hellrookie.ToolKit
{
    public class ExcelHandler
    {
        private static readonly System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.GetProperty;
        public static Dictionary<string, List<Dictionary<string, Object>>> ReadFromFile(string path)
        {
            Dictionary<string, List<Dictionary<string, Object>>> data = new Dictionary<string, List<Dictionary<string, Object>>>();
            IWorkbook wk = GetWorkbook(path);
            if (wk != null)
            {
                for (int i = 0; i < wk.NumberOfSheets; ++i)
                {
                    var sheet = wk.GetSheetAt(i);
                    List<Dictionary<string, Object>> sheetData = GetSheetData(sheet);
                    data.Add(sheet.SheetName, sheetData);
                }
            }
            return data;
        }

        public static List<T> ReadFromSheet<T>(string path, int sheetPosition)
        {
            IWorkbook wk = GetWorkbook(path);
            return ReadFromSheet<T>(wk.GetSheetAt(sheetPosition));
        }

        public static void WriteToFile<T>(string path, List<T> data)
        {
            path = path.EndsWith(".xls") ? path : path + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls";

            var props = AssembleUtil.GetPorpertyNames<T>(flags);

            HSSFWorkbook workbook = new HSSFWorkbook(); 
            ISheet sheet = workbook.CreateSheet(Path.GetFileNameWithoutExtension(path));
            

            Dictionary<int, string> valuePos = new Dictionary<int, string>();
            for(int i = 0; i < props.Count; ++i)
            {
                valuePos.Add(i, props[i]);
            }

            IRow headRow = sheet.CreateRow(0);
            SetRowValue(headRow, valuePos);

            int rowPos = 1;
            foreach(var d in data)
            {
                var row = sheet.CreateRow(rowPos++);
                SetRowValue(row, valuePos, AssembleUtil.GetPropertyValues(d, flags));
            }

            using (FileStream file = new FileStream(path, FileMode.Create))
            {
                workbook.Write(file);
            }
        }

        private static void SetRowValue(IRow row, Dictionary<int, string> valuePos, Dictionary<string, object> values)
        {
            foreach(var p in valuePos.Keys)
            {
                if (values.ContainsKey(valuePos[p]))
                {
                    SetCellValue(row.CreateCell(p), values[valuePos[p]]);
                }
                else
                {
                    SetCellValue(row.CreateCell(p), string.Empty);
                }
            }
        }

        private static void SetRowValue(IRow row, Dictionary<int, string> valuePos)
        {
            foreach (var p in valuePos.Keys)
            {
                SetCellValue(row.CreateCell(p), valuePos[p]);
            }
        }

        private static IWorkbook GetWorkbook(string path)
        {
            string extension = System.IO.Path.GetExtension(path);
            using (FileStream fs = File.OpenRead(path))
            {
                if (extension.Equals(".xls"))
                {
                    //把xls文件中的数据写入wk中
                    return new HSSFWorkbook(fs);
                }
                else if (extension.Equals(".xlsx"))
                {
                    //把xlsx文件中的数据写入wk中
                    return new XSSFWorkbook(fs);
                }
            }
            return null;
        }

        private static List<T> ReadFromSheet<T>(ISheet sheet)
        {
            List<T> formattedData = new List<T>();
            List<Dictionary<string, object>> sheetData = GetSheetData(sheet, true);
            foreach(var d in sheetData)
            {
                formattedData.Add(AssembleUtil.SetPorpertyValues<T>(d, flags));
            }
            return formattedData;
        }

        private static List<Dictionary<string, Object>> GetSheetData(ISheet sheet, bool needRemoveSpaceFromHead = false)
        {
            var head = GetHead(sheet.GetRow(0));
            List<Dictionary<string, Object>> sheetData = new List<Dictionary<string, object>>();
            for (int j = 1; j <= sheet.LastRowNum; ++j)
            {
                sheetData.Add(GetRowData(sheet.GetRow(j), head, needRemoveSpaceFromHead));
            }
            return sheetData;
        }

        private static Dictionary<string, object> GetRowData(IRow row, string[] head, bool needRemoveSpaceFromHead)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            for(int i = 0; i < row.LastCellNum; ++i)
            {
                string h = string.Empty;
                if(head.Length > i)
                {
                    h = head[i];
                }
                else
                {
                    h = string.Format("OutOfRangeHead at {0}", i);
                }
                data.Add((needRemoveSpaceFromHead ? h.Replace(" ", "") : h).ToLower(), GetCellValue(row.GetCell(i)));
            }
            return data;
        }

        private static string[] GetHead(IRow row)
        {
            string[] head = new string[row.LastCellNum];
            for(int i = 0; i < row.LastCellNum; ++i)
            {
                head[i] = row.GetCell(i).ToString();
            }
            return head;
        }

        private static object GetCellValue(ICell cell)
        {
            object value = null;
            try
            {
                if (cell.CellType != CellType.Blank)
                {
                    switch (cell.CellType)
                    {
                        case CellType.Numeric:
                            // Date comes here
                            if (DateUtil.IsCellDateFormatted(cell))
                            {
                                value = cell.DateCellValue;
                            }
                            else
                            {
                                // Numeric type
                                value = cell.NumericCellValue;
                            }
                            break;
                        case CellType.Boolean:
                            // Boolean type
                            value = cell.BooleanCellValue;
                            break;
                        case CellType.Formula:
                            value = cell.CellFormula;
                            break;
                        default:
                            // String type
                            value = cell.StringCellValue;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                value = "";
            }

            return value;
        }

        private static void SetCellValue(ICell cell, object obj)
        {
            if(obj == null)
            {
                cell.SetCellValue(string.Empty);
            }
            else if (obj.GetType() == typeof(int))
            {
                cell.SetCellValue((int)obj);
            }
            else if (obj.GetType() == typeof(double))
            {
                cell.SetCellValue((double)obj);
            }
            else if (obj.GetType() == typeof(IRichTextString))
            {
                cell.SetCellValue((IRichTextString)obj);
            }
            else if (obj.GetType() == typeof(string))
            {
                cell.SetCellValue(obj.ToString());
            }
            else if (obj.GetType() == typeof(DateTime))
            {
                cell.SetCellValue((DateTime)obj);
            }
            else if (obj.GetType() == typeof(bool))
            {
                cell.SetCellValue((bool)obj);
            }
            else
            {
                cell.SetCellValue(obj.ToString());
            }
        }
    }
}
