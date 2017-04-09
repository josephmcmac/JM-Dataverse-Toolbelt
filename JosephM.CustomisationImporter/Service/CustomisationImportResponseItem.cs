using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Metadata;
using System;

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportResponseItem : ServiceResponseItem
    {
        public CustomisationImportResponseItem(IMetadata metadata, bool isUpdate)
        {
            Metadata = metadata;
            Updated = isUpdate;
        }

        public CustomisationImportResponseItem(IMetadata metadata, Exception ex)
        {
            Metadata = metadata;
            Exception = ex;
        }

        public CustomisationImportResponseItem(int? excelRow, IMetadata metadata, bool isUpdate)
        {
            ExcelRow = excelRow;
            Metadata = metadata;
            Updated = isUpdate;
        }

        public CustomisationImportResponseItem(int? excelRow, IMetadata metadata, Exception ex)
        {
            ExcelRow = excelRow;
            Metadata = metadata;
            Exception = ex;
        }

        public int? ExcelRow { get; set; }
        public string Type { get {
                var typeDisplay = Metadata.GetType().GetDisplayName();
                if (typeDisplay.EndsWith(" Metadata"))
                    typeDisplay = typeDisplay.Left(typeDisplay.LastIndexOf(" Metadata"));
                return typeDisplay;
            } }
        public string Name { get { return Metadata.SchemaName; } }
        public bool Updated { get; set; }
        internal IMetadata Metadata { get; set; }
    }
}