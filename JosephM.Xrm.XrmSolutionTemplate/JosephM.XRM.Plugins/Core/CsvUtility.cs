#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#endregion

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

                var propertyNames = typeToOutput.GetReadableProperties().Select(p => p.Name).ToArray();
                var propertyNamesText = String.Join(",", propertyNames);
                FileUtility.CheckCreateFile(path, name, propertyNamesText);

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine();

                foreach (var o in objects)
                {
                    var thisObjectType = o.GetType();
                    var valueStrings = new List<string>();
                    foreach (var property in propertyNames)
                    {
                        object value = null;
                        if (thisObjectType.GetProperty(property) != null)
                            value = o.GetPropertyValue(property);
                        if (value == null)
                            valueStrings.Add("");
                        else
                        {
                            var thisString = value.ToString();
                            thisString = thisString.Replace("\n", CsvNewLine);
                            thisString = thisString.Replace("\"", "\"\"");
                            valueStrings.Add("\"" + thisString + "\"");
                        }
                    }
                    stringBuilder.AppendLine(string.Join(",", valueStrings));
                }
                FileUtility.AppendToFile(path, name, stringBuilder.ToString());
            }
        }

        /// <summary>
        ///     Outputs where the type uses csvattributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static string CreateCsvText<T>(IEnumerable<T> objects)
        {
            var headerToPropertyNameMaps = new SortedDictionary<int, KeyValuePair<string, string>>();
            var preoperties = typeof(T).GetProperties();
            foreach (var property in preoperties)
            {
                var customerAttribute = property.GetCustomAttribute<CsvAttribute>();
                if (customerAttribute != null)
                {
                    if (headerToPropertyNameMaps.ContainsKey(customerAttribute.Order))
                        throw new Exception(
                            string.Format(
                                "Type {0} Has Multiple Properties With {1} Order Of {2}. The Order Must Be Unique"
                                , typeof(T).Name, typeof(CsvAttribute).Name, customerAttribute.Order));
                    headerToPropertyNameMaps.Add(customerAttribute.Order,
                        new KeyValuePair<string, string>(customerAttribute.Label, property.Name));
                }
            }

            var propertyNamesText = String.Join(",",
                headerToPropertyNameMaps.OrderBy(i => i.Key).Select(i => i.Value.Key));
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(propertyNamesText);

            foreach (var o in objects)
            {
                var thisObjectType = o.GetType();
                var valueStrings = new List<string>();
                foreach (var property in headerToPropertyNameMaps.OrderBy(i => i.Key))
                {
                    object value = null;
                    if (thisObjectType.GetProperty(property.Value.Value) != null)
                        value = o.GetPropertyValue(property.Value.Value);
                    if (value == null)
                        valueStrings.Add("");
                    else
                    {
                        var thisString = value.ToString();
                        thisString = thisString.Replace("\n", CsvNewLine);
                        thisString = thisString.Replace("\"", "\"\"");
                        valueStrings.Add("\"" + thisString + "\"");
                    }
                }
                stringBuilder.AppendLine(string.Join(",", valueStrings));
            }
            return stringBuilder.ToString();
        }

        private static string GetCsvFileName(string name)
        {
            return name.ToLower().EndsWith(".csv") ? name : name + ".csv";
        }

        public class CsvAttribute : Attribute
        {
            public int Order { get; set; }
            public string Label { get; set; }

            public CsvAttribute(int order, string label)
            {
                Label = label;
                Order = order;
            }
        }
    }
}