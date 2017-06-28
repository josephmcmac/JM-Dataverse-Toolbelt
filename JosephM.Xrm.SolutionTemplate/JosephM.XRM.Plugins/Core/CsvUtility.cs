#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;
using System.Data.OleDb;
using System.Data;
using System.IO;

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

        public static void CreateCsv(string path, string name, IEnumerable objects, IEnumerable<string> propertyNames, Func<string, string> getLabel, Func<object, string, object> getField)
        {
            var propertyLabels = propertyNames.Select(getLabel).ToArray();
            var propertyNamesText = String.Join(",", propertyLabels);
            FileUtility.CheckCreateFile(path, name, propertyNamesText);

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
            FileUtility.AppendToFile(path, name, stringBuilder.ToString());
        }

        private static string GetCsvFileName(string name)
        {
            return name.ToLower().EndsWith(".csv") ? name : name + ".csv";
        }

        /// <summary>
        /// Generates a Schema.ini fiel for the csv specifiying all columns as type text
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        public static void ConstructTextSchema(string folder, string fileName)
        {
            if (folder.IsNullOrWhiteSpace())
                folder = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(folder + @"\Schema.ini"))
                File.Delete(folder + @"\Schema.ini");
            var schema = new StringBuilder();
            var fields = GetColumns(Path.Combine(folder, fileName)).ToList();
            schema.AppendLine("[" + fileName + "]");
            schema.AppendLine("ColNameHeader=True");
            for (var i = 0; i < fields.Count(); i++)
            {
                schema.AppendLine("col" + (i + 1) + "=\"" + fields[i] + "\" Memo");
            }
            FileUtility.WriteToFile(folder, "Schema.ini", schema.ToString());
        }

        public static IEnumerable<string> GetColumns(string fileNameQualified)
        {
            var sql = string.Format("select top 1 * from [{0}]", GetTableName(fileNameQualified));
            var result = SelectRows(fileNameQualified, sql);
            if (!result.Any())
                throw new NullReferenceException(string.Format("Error examining CSV. There were no rows returned in the csv {0}", fileNameQualified));
            var row = result.First();
            return row.GetColumnNames();
        }

        public static IEnumerable<QueryRow> SelectRows(string fileNameQualified, string sql)
        {
            return SelectRows(GetFolder(fileNameQualified), GetFileName(fileNameQualified), sql);
        }

        public static IEnumerable<QueryRow> SelectRows(string folder, string fileName, string sql)
        {
            OleDbDataAdapter dAdapter = null;
            try
            {
                if (folder.IsNullOrWhiteSpace())
                    folder = AppDomain.CurrentDomain.BaseDirectory;
                var connString = GetConnectionString(folder);
                var dTable = new DataTable();
                dAdapter = new OleDbDataAdapter(sql, connString);
                dAdapter.Fill(dTable);
                var itemsToAdd = new List<DataRow>();
                foreach (DataRow row in dTable.Rows)
                {
                    itemsToAdd.Add(row);
                }
                return itemsToAdd.Select(r => new QueryRow(r)).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading from csv tab\nFolder: " + folder + "\nFile: " + fileName + "\n" +
                                    ex.DisplayString());
            }
            finally
            {
                if (dAdapter != null)
                {
                    dAdapter.Dispose();
                }
            }
        }

        private static string GetConnectionString(string path)
        {
            var connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;"
                                   + "Data Source=\"" + path + "\\\";"
                                   + "Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
            return connectionString;
        }

        public static IEnumerable<QueryRow> SelectAllRows(string fileNameQualified)
        {
            return SelectRows(GetFolder(fileNameQualified), GetFileName(fileNameQualified),
                "select * from [" + GetTableName(fileNameQualified) + "]");
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