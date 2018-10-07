#region

using System;
using JosephM.Core.Service;
using JosephM.Record.Metadata;
using JosephM.Core.Attributes;
using System.Collections.Generic;
using JosephM.Core.Extentions;
using System.Linq;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportResponse : ServiceResponseBase<CustomisationImportResponseItem>
    {
        [Hidden]
        public bool ExcelReadErrors { get; internal set; }

        private List<ImportedItem> _importedItems = new List<ImportedItem>();

        [Hidden]
        public bool HasImportedItems
        {
            get { return _importedItems.Any(); }
        }

        [PropertyInContextByPropertyValue(nameof(HasImportedItems), true)]
        [AllowDownload]
        public IEnumerable<ImportedItem> ImportedItems
        {
            get { return _importedItems; }
        }

        public void AddImportedItem(IMetadata metadata, bool isUpdate)
        {
            var responseItem = new ImportedItem(metadata, isUpdate);
            _importedItems.Add(responseItem);
        }

        public void AddImportedItem(int excelRow, IMetadata metadata, bool isUpdate)
        {
            var responseItem = new ImportedItem(excelRow, metadata, isUpdate);
            _importedItems.Add(responseItem);
        }

        internal void AddResponseItem(IMetadata metadata, Exception ex)
        {
            var responseItem = new CustomisationImportResponseItem(metadata, ex);
            AddResponseItem(responseItem);
        }

        internal void AddResponseItem(int excelRow, IMetadata metadata, Exception ex)
        {
            var responseItem = new CustomisationImportResponseItem(excelRow, metadata, ex);
            AddResponseItem(responseItem);
        }

        public class ImportedItem
        {
            public ImportedItem(IMetadata metadata, bool isUpdate)
            {
                Metadata = metadata;
                Updated = isUpdate;
            }

            public ImportedItem(int? excelRow, IMetadata metadata, bool isUpdate)
            {
                ExcelRow = excelRow;
                Metadata = metadata;
                Updated = isUpdate;
            }

            [DisplayOrder(10)]
            [GridWidth(125)]
            [PropertyInContextByPropertyNotNull(nameof(ExcelRow))]
            public int? ExcelRow { get; }
            [DisplayOrder(20)]
            public string Type
            {
                get
                {
                    var typeDisplay = Metadata?.GetType().GetDisplayName();
                    if (typeDisplay == null)
                        return null;
                    if (typeDisplay.EndsWith(" Metadata"))
                        typeDisplay = typeDisplay.Left(typeDisplay.LastIndexOf(" Metadata"));
                    return typeDisplay;
                }
            }
            [DisplayOrder(30)]
            public string Name { get { return Metadata?.SchemaName; } }
            [DisplayOrder(40)]
            [GridWidth(125)]
            public bool Updated { get; set; }
            internal IMetadata Metadata { get; set; }
        }
    }
}