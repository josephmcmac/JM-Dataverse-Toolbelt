#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

#endregion

namespace JosephM.Deployment.ExportXml
{
    [DisplayName("Export XML")]
    [Instruction("A Folder Will Be Created Containing An Xml File For Each Record Included. The Import XML Process May Then Be Run On That Folder To Import The Data Into A Dynamics Instance")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.IncludeWithExportedRecords, true, 30)]
    [Group(Sections.Misc, true, 40)]
    public class ExportXmlRequest : ServiceRequestBase
    {
        [GridWidth(300)]
        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [DisplayName("Select The Folder To Export The XML Files Into")]
        public Folder Folder { get; set; }

        [GridWidth(110)]
        [DisplayOrder(200)]
        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include Attached Notes")]
        [RequiredProperty]
        public bool IncludeNotes { get; set; }

        [GridWidth(110)]
        [DisplayOrder(210)]
        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include N:N Links Between Records")]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [GridWidth(500)]
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