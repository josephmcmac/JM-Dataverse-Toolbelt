using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Xrm.DataImportExport.XmlImport;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

namespace JosephM.Xrm.MigrateRecords
{
    [Instruction("Records will be queried from the source instance, then upserted into the target intance")]
    [AllowSaveAndLoad]
    [Group(Sections.Connections, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.RecordTypes, Group.DisplayLayoutEnum.VerticalList, order: 20)]
    [Group(Sections.ImportOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30)]
    [Group(Sections.CacheOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40)]
    public class MigrateRecordsRequest : ServiceRequestBase
    {
        public MigrateRecordsRequest()
        {
            MatchByName = true;
            IncludeNNRelationshipsBetweenEntities = true;
            IncludeNotes = true;
            ExecuteMultipleSetSize = 50;
            TargetCacheLimit = 1000;
        }

        [MyDescription("Connection for the instance to migrate record from")]
        [Group(Sections.Connections)]
        [DisplayOrder(10)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections), allowAddNew: false)]
        [ConnectionFor(nameof(RecordTypesToMigrate))]
        public SavedXrmRecordConfiguration SourceConnection { get; set; }

        [MyDescription("Connection for the instance to migrate record into")]
        [Group(Sections.Connections)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections), allowAddNew: false)]
        public SavedXrmRecordConfiguration TargetConnection { get; set; }

        [GridWidth(110)]
        [Group(Sections.ImportOptions)]
        [DisplayOrder(221)]
        [RequiredProperty]
        public bool IncludeOwner { get; set; }

        [GridWidth(110)]
        [MyDescription("If set any email addrese in contact or account record will be rewritten into fake email addresses")]
        [Group(Sections.ImportOptions)]
        [DisplayOrder(200)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [GridWidth(110)]
        [MyDescription("If set any notes or attachments for records in the migration will alo be migrated")]
        [DisplayOrder(205)]
        [Group(Sections.ImportOptions)]
        [DisplayName("Include notes & attachments")]
        [RequiredProperty]
        public bool IncludeNotes { get; set; }

        [GridWidth(110)]
        [Group(Sections.ImportOptions)]
        [DisplayOrder(207)]

        [DisplayName("Include file & image fields")]
        public bool IncludeFileAndImageFields { get; set; }

        [GridWidth(110)]
        [MyDescription("If et an N to N associations between record included in the migration will also be migrated")]
        [DisplayOrder(210)]
        [Group(Sections.ImportOptions)]
        [DisplayName("Include n:n links between records")]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [GridWidth(110)]
        [DisplayOrder(215)]
        [Group(Sections.ImportOptions)]
        [RequiredProperty]
        public bool MatchByName { get; set; }

        [Group(Sections.CacheOptions)]
        [DisplayOrder(222)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        public int? ExecuteMultipleSetSize { get; set; }

        [Group(Sections.CacheOptions)]
        [DisplayOrder(225)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(5000)]
        public int? TargetCacheLimit { get; set; }

        [Group(Sections.RecordTypes)]
        [GridWidth(500)]
        [MyDescription("The specific records to migrate")]
        [PropertyInContextByPropertyNotNull(nameof(SourceConnection))]
        [DisplayOrder(300)]
        [RequiredProperty]
        [AllowGridFullScreen]
        public IEnumerable<ExportRecordType> RecordTypesToMigrate { get; set; }

        private static class Sections
        {
            public const string Connections = "Connections";
            public const string RecordTypes = "Records to Migrate";
            public const string ImportOptions = "Import Options";
            public const string CacheOptions = "Cache Options";
        }
    }
}