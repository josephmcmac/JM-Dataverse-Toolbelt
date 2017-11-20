#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Deployment.ExportXml;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

#endregion

namespace JosephM.Deployment.MigrateRecords
{
    [AllowSaveAndLoad]
    [Group(Sections.Connections, true, 10)]
    [Group(Sections.IncludeWithExportedRecords, true, 30)]
    public class MigrateRecordsRequest : ServiceRequestBase
    {
        [Group(Sections.Connections)]
        [DisplayOrder(10)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        [ConnectionFor(nameof(RecordTypesToMigrate))]
        public SavedXrmRecordConfiguration SourceConnection { get; set; }

        [Group(Sections.Connections)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration TargetConnection { get; set; }

        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayOrder(200)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [DisplayOrder(205)]
        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include Notes Attached To Exported Records")]
        [RequiredProperty]
        public bool IncludeNotes { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include N:N Associations Between Exported Records")]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [PropertyInContextByPropertyNotNull(nameof(SourceConnection))]
        [DisplayOrder(300)]
        [RequiredProperty]
        public IEnumerable<ExportRecordType> RecordTypesToMigrate { get; set; }

        private static class Sections
        {
            public const string Connections = "Connections";
            public const string IncludeWithExportedRecords = "Options To Include With Exported Records";
        }
    }
}