using JosephM.Core.Extentions;
using JosephM.Deployment.DataImport;
using System;
using System.IO;

namespace JosephM.Deployment.ImportSql
{
    public class ImportSqlResponseItem : DataImportResponseItem
    {
        public string FileName 
        {
            get { return FileNameQualified.IsNullOrWhiteSpace() ? null : Path.GetFileName(FileNameQualified); }
        }

        private string FileNameQualified { get; set; }

        public ImportSqlResponseItem(string message, string fileNameQualified, Exception ex)
            : base(message, ex)
        {
            FileNameQualified = fileNameQualified;
        }

        public ImportSqlResponseItem(DataImportResponseItem item)
            : base(item.Entity, item.Field, item.Name, item.FieldValue, item.Message, item.Exception, rowNumber: item.RowNumber)
        {
        }
    }
}