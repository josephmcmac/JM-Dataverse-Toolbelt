#region

using JosephM.Core.FieldType;
using JosephM.Core.Service;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportRequest : ServiceRequestBase
    {
        public ExcelFile ExcelFile { get; set; }
        public bool IncludeEntities { get; set; }
        public bool IncludeFields { get; set; }
        public bool IncludeRelationships { get; set; }
        public bool UpdateOptionSets { get; set; }
        public bool UpdateViews { get; set; }
    }
}