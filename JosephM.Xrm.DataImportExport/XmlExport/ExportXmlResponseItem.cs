using JosephM.Xrm.DataImportExport.Import;
using System;
using System.IO;

namespace JosephM.Xrm.DataImportExport.XmlImport
{
    public class ExportXmlResponseItem : DataImportResponseItem
    {
        public string FileName 
        {
            get { return string.IsNullOrWhiteSpace(FileNameQualified) ? null : Path.GetFileName(FileNameQualified); }
        }

        private string FileNameQualified { get; set; }

        public ExportXmlResponseItem(string message, string fileNameQualified, Exception ex)
            : base(message, ex)
        {
            FileNameQualified = fileNameQualified;
        }
    }
}