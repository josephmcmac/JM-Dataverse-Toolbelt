using JosephM.Core.Extentions;
using JosephM.Xrm.DataImportExport.Import;
using System;
using System.IO;

namespace JosephM.Xrm.CsvImport
{
    public class CsvImportResponseItem : DataImportResponseItem
    {
        public string FileName 
        {
            get { return FileNameQualified.IsNullOrWhiteSpace() ? null : Path.GetFileName(FileNameQualified); }
        }

        private string FileNameQualified { get; set; }

        public CsvImportResponseItem(string message, string fileNameQualified, Exception ex)
            : base(message, ex)
        {
            FileNameQualified = fileNameQualified;
        }

        public CsvImportResponseItem(DataImportResponseItem item)
            : base(item.Entity, item.Field, item.Name, item.FieldValue, item.Message, item.Exception, rowNumber: item.RowNumber)
        {
        }
    }
}