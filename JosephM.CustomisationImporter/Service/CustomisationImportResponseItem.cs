using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Metadata;
using System;

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportResponseItem : ServiceResponseItem
    {
        public CustomisationImportResponseItem(IMetadata metadata, Exception ex)
        {
            Metadata = metadata;
            Exception = ex;
        }

        public CustomisationImportResponseItem(int? excelRow, IMetadata metadata, Exception ex)
        {
            ExcelRow = excelRow;
            Metadata = metadata;
            Exception = ex;
        }

        [DisplayOrder(10)]
        [GridWidth(125)]
        [PropertyInContextByPropertyNotNull(nameof(ExcelRow))]
        public int? ExcelRow { get; }
        [DisplayOrder(20)]
        public string Type { get {
                var typeDisplay = Metadata?.GetType().GetDisplayName();
                if (typeDisplay == null)
                    return null;
                if (typeDisplay.EndsWith(" Metadata"))
                    typeDisplay = typeDisplay.Left(typeDisplay.LastIndexOf(" Metadata"));
                return typeDisplay;
            } }
        [DisplayOrder(30)]
        public string Name { get { return Metadata?.SchemaName; } }
        [Hidden]
        internal IMetadata Metadata { get; set; }
    }
}