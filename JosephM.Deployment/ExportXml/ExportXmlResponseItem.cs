#region

using System;
using System.IO;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Record.IService;

#endregion

namespace JosephM.Deployment.ExportXml
{
    public class ExportXmlResponseItem : DataImportResponseItem
    {
        public string FileName 
        {
            get { return FileNameQualified.IsNullOrWhiteSpace() ? null : Path.GetFileName(FileNameQualified); }
        }

        private string FileNameQualified { get; set; }

        public ExportXmlResponseItem(string message, string fileNameQualified, Exception ex)
            : base(message, ex)
        {
            FileNameQualified = fileNameQualified;
        }
    }
}