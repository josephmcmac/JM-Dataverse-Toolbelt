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
        public string Entity { get; set; }

        public string Name { get; set; }

        public string Field { get; set; }

        public string FileName 
        {
            get { return FileNameQualified.IsNullOrWhiteSpace() ? null : Path.GetFileName(FileNameQualified); }
        }

        private string FileNameQualified { get; set; }

        public string Message { get; set; }

        public XrmImporterExporterResponseItem(string message, string fileNameQualified, Exception ex)
            : this(message, ex)
        {
            FileNameQualified = fileNameQualified;
        }

        public XrmImporterExporterResponseItem(string entity, string field, string name, string message, Exception ex)
            : this(message, ex)
        {
            Entity = entity;
            Field = field;
            Name = name;
        }

        public XrmImporterExporterResponseItem(string message, Exception ex)
        {
            Message = message;
            Exception = ex;
        }
    }
}