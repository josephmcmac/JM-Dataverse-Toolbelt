#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Sql;

#endregion

namespace JosephM.Core.Utility
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
                var propertyLabels = typeToOutput.GetReadableProperties().Select(s => s.GetDisplayName()).ToArray();
                var propertyNamesText = String.Join(",", propertyLabels);
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

        private static string GetCsvFileName(string name)
        {
            return name.ToLower().EndsWith(".csv") ? name : name + ".csv";
        }

        public static void ConstructTextSchema(string folder, string fileName)
        {
            if (folder.IsNullOrWhiteSpace())
                folder = AppDomain.CurrentDomain.BaseDirectory;
            var schema = new StringBuilder();
            var data = LoadCSV(folder, fileName);
            schema.AppendLine("[" + fileName + "]");
            schema.AppendLine("ColNameHeader=True");
            for (var i = 0; i < data.Columns.Count; i++)
            {
                schema.AppendLine("col" + (i + 1).ToString() + "=\"" + data.Columns[i].ColumnName + "\" Text");
            }
            var schemaFileName = folder + @"\Schema.ini";
            TextWriter tw = new StreamWriter(schemaFileName);
            tw.WriteLine(schema.ToString());
            tw.Close();
        }

        public static DataTable LoadCSV(string folder, string fileName)
        {
            if (folder.IsNullOrWhiteSpace())
                folder = AppDomain.CurrentDomain.BaseDirectory;
            var sqlString = "Select * FROM [" + fileName + "];";
            var conStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
                         + folder + ";" + "Extended Properties='text;HDR=YES;'";
            var theCSV = new DataTable();

            using (var conn = new OleDbConnection(conStr))
            {
                using (var comm = new OleDbCommand(sqlString, conn))
                {
                    using (var adapter = new OleDbDataAdapter(comm))
                    {
                        adapter.Fill(theCSV);
                    }
                }
            }
            return theCSV;
        }

        public static DataTable SelectFromExcelTabName(string folder, string fileName)
        {
            OleDbDataAdapter dAdapter = null;
            try
            {
                if (folder.IsNullOrWhiteSpace())
                    folder = AppDomain.CurrentDomain.BaseDirectory;
                var connString = GetConnectionString(folder);
                var excelQuery = "select * from [" + fileName + "]";
                var dTable = new DataTable();
                dAdapter = new OleDbDataAdapter(excelQuery, connString);
                dAdapter.Fill(dTable);
                return dTable;
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

        public static IEnumerable<QueryRow> SelectPropertyBagsFromCsv(string fileNameQualified)
        {
            return SelectRowsFromCsvTabName(Path.GetDirectoryName(fileNameQualified), Path.GetFileName(fileNameQualified)).Select(r => new QueryRow(r));
        }

        public static IEnumerable<QueryRow> SelectPropertyBagsFromCsv(string folder, string fileName)
        {
            return SelectRowsFromCsvTabName(folder, fileName).Select(r => new QueryRow(r)).ToArray();
        }

        private static IEnumerable<DataRow> SelectRowsFromCsvTabName(string folder, string fileName)
        {
            var allData = SelectFromExcelTabName(folder, fileName);
            var itemsToAdd = new List<DataRow>();
            foreach (DataRow row in allData.Rows)
            {
                itemsToAdd.Add(row);
            }
            return itemsToAdd;
        }
    }
}