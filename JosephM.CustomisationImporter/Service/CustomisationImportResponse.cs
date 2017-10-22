#region

using System;
using JosephM.Core.Service;
using JosephM.Record.Metadata;
using JosephM.Core.Attributes;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportResponse : ServiceResponseBase<CustomisationImportResponseItem>
    {
        [Hidden]
        public bool ExcelReadErrors { get; internal set; }

        public void AddResponseItem(IMetadata metadata, bool isUpdate)
        {
            var responseItem = new CustomisationImportResponseItem(metadata, isUpdate);
            AddResponseItem(responseItem);
        }

        public void AddResponseItem(int excelRow, IMetadata metadata, bool isUpdate)
        {
            var responseItem = new CustomisationImportResponseItem(excelRow, metadata, isUpdate);
            AddResponseItem(responseItem);
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
    }
}