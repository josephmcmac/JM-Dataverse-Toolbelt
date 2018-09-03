using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.ExportXml;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

namespace JosephM.Deployment.MigrateRecords
{
    [Instruction("All Records Will Be Queried From The Source Instance, Then Imported Into The Target Instance. Matches To Update Records In The Target Will By Done By Either Primary Key, Then Name, Else If No Match Is Found A New Record Will Be Created")]
    [AllowSaveAndLoad]
    [Group(Sections.Connections, true, 10)]
    [Group(Sections.RecordTypesOptions, true, 30)]
    [Group(Sections.RecordTypes, true, order: 35, displayLabel: false)]
    public class MigrateRecordsRequest : ServiceRequestBase
    {
        [MyDescription("The Connection Which Records Will Be Migrated From")]
        [Group(Sections.Connections)]
        [DisplayOrder(10)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        [ConnectionFor(nameof(RecordTypesToMigrate))]
        public SavedXrmRecordConfiguration SourceConnection { get; set; }

        [MyDescription("The Connection Which Records Will Be Migrated Into")]
        [Group(Sections.Connections)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration TargetConnection { get; set; }

        [GridWidth(110)]
        [MyDescription("If Set Any Email Addresses In Contact Or Account Records Will Be Rewritten To Fake Email Addresses")]
        [Group(Sections.RecordTypesOptions)]
        [DisplayOrder(200)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [GridWidth(110)]
        [MyDescription("If Set Any Notes Attached To Records Will Be Included In The Migration")]
        [DisplayOrder(205)]
        [Group(Sections.RecordTypesOptions)]
        [DisplayName("Include Attached Notes")]
        [RequiredProperty]
        public bool IncludeNotes { get; set; }

        [GridWidth(110)]
        [MyDescription("If Set Any N:N Relationship Associations Between Records Being Migrated WIll Be Included In The Migration")]
        [DisplayOrder(210)]
        [Group(Sections.RecordTypesOptions)]
        [DisplayName("Include N:N Links Between Records")]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [Group(Sections.RecordTypes)]
        [GridWidth(500)]
        [MyDescription("The Specific Records Configured To Be included In The Migrated")]
        [PropertyInContextByPropertyNotNull(nameof(SourceConnection))]
        [DisplayOrder(300)]
        [RequiredProperty]
        public IEnumerable<ExportRecordType> RecordTypesToMigrate { get; set; }

        private static class Sections
        {
            public const string Connections = "Connections";
            public const string RecordTypesOptions = "Record Types To Migrate";
            public const string RecordTypes = "Record Types";
        }
    }
}