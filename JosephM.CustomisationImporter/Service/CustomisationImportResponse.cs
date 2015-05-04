#region

using System;
using JosephM.Core.Service;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportResponse : ServiceResponseBase<CustomisationImportResponseItem>
    {
        public void AddResponseItem(string type, string name, bool isUpdate)
        {
            var responseItem = new CustomisationImportResponseItem {Name = name, Type = type, Updated = isUpdate};
            AddResponseItem(responseItem);
        }

        internal void AddResponseItem(string type, string name, Exception ex)
        {
            var responseItem = new CustomisationImportResponseItem {Name = name, Type = type, Exception = ex};
            AddResponseItem(responseItem);
        }
    }
}