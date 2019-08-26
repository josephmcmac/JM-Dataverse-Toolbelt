using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace $safeprojectname$.Core
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
            name = GetCsvFileName(name);

            if (objects != null && objects.Any(o => o != null))
            {
                var typeToOutput = objects.First(o => o != null).GetType();

                var propertyNames = typeToOutput.GetReadableProperties().Select(s => s.Name).ToArray();

                CreateCsv(path, name, objects, propertyNames
                    , delegate (string s)
                {
                    return typeToOutput.GetProperty(s).Name;
                }, delegate (object o, string s)
                {
                    return o.GetPropertyValue(s);
                });
            }
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
    }
}