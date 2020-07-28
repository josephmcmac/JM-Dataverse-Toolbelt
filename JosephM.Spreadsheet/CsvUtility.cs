using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Sql;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace JosephM.Spreadsheet
{
    public class CsvUtility
    {
        private const string CsvNewLine = @"
";

        public static void CreateCsv<T>(string path, string name, IEnumerable<T> objects)
        {
            CreateCsv(path, name, objects, new LogController());
        }

        public static void CreateCsv<T>(string path, string name, IEnumerable<T> objects, LogController ui)
        {
            if (objects != null && objects.Any(o => o != null))
            {
                var typeToOutput = objects.First(o => o != null).GetType();

                var propertyNames = typeToOutput.GetReadableProperties().Select(s => s.Name).ToArray();

                CreateCsv(path, name, objects, propertyNames
                    , delegate (string s)
                {
                    return typeToOutput.GetProperty(s).GetDisplayName();
                }, delegate (object o, string s)
                {
                    return o.GetPropertyValue(s);
                });
            }
        }

        public static void CreateCsv(string path, string name, IEnumerable sheet)
        {
            name = GetCsvFileName(name);

            var typeToOutput = sheet.GetType().GenericTypeArguments[0];
            var propertyNames = typeToOutput.GetReadableProperties().Select(s => s.Name).ToArray();
            Func<string, string> getLabel = (s) => typeToOutput.GetProperty(s).GetDisplayName();
            Func<object, string, object> getField = (o, s) => o.GetPropertyValue(s);

            CreateCsv(path, name, sheet, propertyNames, getLabel, getField);
        }

        public static void CreateCsv(string path, string name, IEnumerable objects, IEnumerable<string> propertyNames, Func<string, string> getLabel, Func<object, string, object> getField)
        {
            var propertyLabels = propertyNames.Select(getLabel).ToArray();
            var propertyNamesText = String.Join(",", propertyLabels);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var filePath = path + @"\" + name;
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.IsReadOnly)
                    fileInfo.IsReadOnly = false;
            }
            File.WriteAllText(path + @"\" + name, propertyNamesText);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();

            foreach (var o in objects)
            {
                var valueStrings = new List<string>();
                foreach (var property in propertyNames)
                {
                    object value = getField(o, property);
                    if (value == null)
                        valueStrings.Add("");
                    else
                    {
                        var thisString = value.ToString() ?? "";
                        thisString = thisString.Replace("\n", CsvNewLine);
                        thisString = thisString.Replace("\"", "\"\"");
                        valueStrings.Add("\"" + thisString + "\"");
                    }
                }
                stringBuilder.AppendLine(string.Join(",", valueStrings));
            }
            File.AppendAllText(path + @"\" + name, stringBuilder.ToString());
        }

        private static string GetCsvFileName(string name)
        {
            return name.ToLower().EndsWith(".csv") ? name : name + ".csv";
        }

        public static IEnumerable<string> GetColumns(string fileNameQualified)
        {
            var result = SelectAllRows(fileNameQualified);
            if (!result.Any())
                throw new NullReferenceException(string.Format("Error examining CSV. There were no rows returned in the csv {0}", fileNameQualified));
            var row = result.First();
            return row.GetColumnNames();
        }

        public static IEnumerable<QueryRow> SelectRows(string fileNameQualified, string sql)
        {
            return SelectAllRows(fileNameQualified);
        }

        public static IEnumerable<QueryRow> SelectRows(string folder, string fileName, string sql)
        {
            return SelectAllRows(Path.Combine(folder, fileName));
        }

        public static IEnumerable<QueryRow> SelectAllRows(string fileNameQualified)
        {
            var dt = new DataTable();
            using (var csv = new CsvReader(new StreamReader(fileNameQualified), true))
            {
                dt.Load(csv);
            }
            var itemsToAdd = new List<DataRow>();
            foreach (DataRow row in dt.Rows)
            {
                itemsToAdd.Add(row);
            }
            return itemsToAdd.Select(r => new QueryRow(r)).ToArray();
        }

        private static string GetFolder(string fileNameQualified)
        {
            return Path.GetDirectoryName(fileNameQualified);
        }

        private static string GetFileName(string fileNameQualified)
        {
            return Path.GetFileName(fileNameQualified);
        }

        public static string GetTableName(string fileNameQualified)
        {
            return Path.GetFileName(fileNameQualified);
        }
    }
}