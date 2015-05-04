#region

using System;
using System.IO;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.IService;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    public class XrmImporterExporterResponseItem : ServiceResponseItem
    {
        public string Message { get; set; }

        public string FileName 
        {
            get { return FileNameQualified.IsNullOrWhiteSpace() ? null : Path.GetFileName(FileNameQualified); }
        }

        private string FileNameQualified { get; set; }

        public XrmImporterExporterResponseItem(string message, string fileNameQualified, Exception ex)
            : this(message, ex)
        {
            FileNameQualified = fileNameQualified;
        }

        public XrmImporterExporterResponseItem(string message, Exception ex)
        {
            Message = message;
            Exception = ex;
        }
    }
}