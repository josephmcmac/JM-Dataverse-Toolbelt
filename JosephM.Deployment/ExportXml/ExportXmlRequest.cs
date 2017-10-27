#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

#endregion

namespace JosephM.Deployment.ExportXml
{
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.IncludeWithExportedRecords, true, 30)]
    [Group(Sections.Misc, true, 40)]
    public class ExportXmlRequest : ServiceRequestBase
    {
        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [DisplayName("Select The Folder To Export The XML Files Into")]
        public Folder Folder { get; set; }

        [DisplayOrder(200)]
        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include Notes Attached To Exported Records")]
        [RequiredProperty]
        public bool IncludeNotes { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include N:N Associations Between Exported Records")]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [DisplayOrder(300)]
        [RequiredProperty]
        public IEnumerable<ExportRecordType> RecordTypesToExport { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Misc = "Misc";
            public const string IncludeWithExportedRecords = "Options To Include With Exported Records";
        }
    }
}