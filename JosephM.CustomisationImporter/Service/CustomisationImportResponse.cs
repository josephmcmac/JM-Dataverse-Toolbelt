#region

using System;
using JosephM.Core.Service;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportResponse : ServiceResponseBase<CustomisationImportResponseItem>
    {
        public bool ExcelReadErrors { get; internal set; }

        public void AddResponseItem(string type, string name, bool isUpdate)
        {
            var responseItem = new CustomisationImportResponseItem {Name = name, Type = type, Updated = isUpdate};
            AddResponseItem(responseItem);
        }

        public void AddResponseItem(int excelRow, string type, string name, bool isUpdate)
        {
            var responseItem = new CustomisationImportResponseItem { ExcelRow = excelRow, Name = name, Type = type, Updated = isUpdate };
            AddResponseItem(responseItem);
        }

        internal void AddResponseItem(string type, string name, Exception ex)
        {
            var responseItem = new CustomisationImportResponseItem {Name = name, Type = type, Exception = ex};
            AddResponseItem(responseItem);
        }

        internal void AddResponseItem(int excelRow, string type, string name, Exception ex)
        {
            var responseItem = new CustomisationImportResponseItem { ExcelRow = excelRow, Type = type, Name = name, Exception = ex };
            AddResponseItem(responseItem);
        }
    }
}