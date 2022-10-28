using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Sql;
using JosephM.Xrm.DataImportExport.MappedImport;
using JosephM.Xrm.Schema;
using System.Collections.Generic;

namespace JosephM.Xrm.SqlImport
{
    [Instruction("This feature has been implemented for an OLEDB connection to a SQL Server instance so you must enter a valid OLEDB connection string\n\n" +
        "This example connection string is for a SQL Server database on the local machine using the current windows login\nProvider=sqloledb;Data Source=localhost;Initial Catalog=sourcedatabasename;Integrated Security=SSPI;\n\n" +
        "Also note being a standalone application this implementation is not designed for high volumes of data. All imported data is loaded into the applications runtime and if the volume is excessive out of memory errors will occur")]
    [DisplayName("Import SQL")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, Group.DisplayLayoutEnum.HorizontalLabelAbove, 10, displayLabel: false)]
    [Group(Sections.ImportOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20)]
    [Group(Sections.CacheOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30)]
    [Group(Sections.SchedulingOptionsOn, Group.DisplayLayoutEnum.VerticalCentered, order: 40)]
    [Group(Sections.SchedulingOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 50)]
    public class SqlImportRequest : ServiceRequestBase, IValidatableObject
    {
        public SqlImportRequest()
        {
            MatchRecordsByName = true;
            ExecuteMultipleSetSize = 50;
            TargetCacheLimit = 1000;
        }

        [DisplayName("OLEDB Connection String")]
        [DisplayOrder(15)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [ConnectionFor(nameof(Mappings) + "." + nameof(SqlImportTableMapping.SourceTable), typeof(SqlConnectionString))]
        [ConnectionFor(nameof(Mappings) + "." + nameof(SqlImportTableMapping.Mappings) + "." + nameof(SqlImportTableMapping.SqlImportFieldMapping.SourceColumn), typeof(SqlConnectionString))]
        public string ConnectionString { get; set; }

        [Group(Sections.SchedulingOptionsOn)]
        [DisplayOrder(500)]
        [RequiredProperty]
        public bool SendNotificationAtCompletion { get; set; }

        [Group(Sections.SchedulingOptions)]
        [DisplayOrder(500)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(SendNotificationAtCompletion), true)]
        public bool OnlySendNotificationIfError { get; set; }

        [Group(Sections.SchedulingOptions)]
        [DisplayOrder(510)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(SendNotificationAtCompletion), true)]
        [ReferencedType(Entities.queue)]
        [UsePicklist]
        public Lookup SendNotificationFromQueue { get; set; }

        [Group(Sections.SchedulingOptions)]
        [DisplayOrder(510)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(SendNotificationAtCompletion), true)]
        [ReferencedType(Entities.queue)]
        [UsePicklist]
        public Lookup SendNotificationToQueue { get; set; }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(410)]
        [RequiredProperty]
        public bool MatchRecordsByName { get; set; }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(415)]
        [RequiredProperty]
        public bool UpdateOnly { get; set; }

        [Group(Sections.CacheOptions)]
        [DisplayOrder(420)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        public int? ExecuteMultipleSetSize { get; set; }

        [Group(Sections.CacheOptions)]
        [DisplayOrder(425)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(5000)]
        public int? TargetCacheLimit { get; set; }

        [AllowGridFullScreen]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(ConnectionString))]
        public IEnumerable<SqlImportTableMapping> Mappings { get; set; }

        public IsValidResponse Validate()
        {
            if (Mappings == null)
                return new IsValidResponse();

            return Mappings.Validate(MatchRecordsByName, UpdateOnly);
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string ImportOptions = "Import Options";
            public const string CacheOptions = "Cache Options";
            public const string SchedulingOptionsOn = "Scheduling Options - Selected Queues Should Have Email Addresses Populated And The From Queue Approved For Sending Emails";
            public const string SchedulingOptions = "Scheduling Options";
        }

        [DoNotAllowGridOpen]
        [Group(Sections.Main, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
        public class SqlImportTableMapping : IMapSourceImport
        {
            [Group(Sections.Main)]
            [DisplayOrder(5)]
            [RequiredProperty]
            [GridWidth(85)]
            public bool IgnoreDuplicates { get; set; }

            [Group(Sections.Main)]
            [DisplayOrder(10)]
            [RequiredProperty]
            [RecordTypeFor(nameof(Mappings) + "." + nameof(SqlImportFieldMapping.SourceColumn))]
            public RecordType SourceTable { get; set; }

            [Group(Sections.Main)]
            [DisplayOrder(20)]
            [RequiredProperty]
            [IncludeManyToManyIntersects]
            [RecordTypeFor(nameof(Mappings) + "." + nameof(SqlImportFieldMapping.TargetField))]
            [RecordTypeFor(nameof(AltMatchKeys) + "." + nameof(SqlImportMatchKey.TargetField))]
            public RecordType TargetType { get; set; }

            [AllowNestedGridEdit]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<SqlImportMatchKey> AltMatchKeys { get; set; }

            [AllowNestedGridEdit]
            [RequiredProperty]
            [GridWidth(800)]
            [PropertyInContextByPropertyNotNull(nameof(SourceTable))]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<SqlImportFieldMapping> Mappings { get; set; }

            string IMapSourceImport.SourceType => SourceTable?.Key;
            string IMapSourceImport.TargetType => TargetType?.Key;
            string IMapSourceImport.TargetTypeLabel => TargetType?.Value;
            IEnumerable<IMapSourceMatchKey> IMapSourceImport.AltMatchKeys => AltMatchKeys;
            IEnumerable<IMapSourceField> IMapSourceImport.FieldMappings => Mappings;

            public override string ToString()
            {
                return (SourceTable?.Value ?? "(None)") + " > " + (TargetType?.Value ?? "(None)");
            }

            private static class Sections
            {
                public const string Main = "Main";
            }

            [DoNotAllowGridOpen]
            public class SqlImportFieldMapping : IMapSourceField
            {
                [RequiredProperty]
                public RecordField SourceColumn { get; set; }

                [RequiredProperty]
                public RecordField TargetField { get; set; }

                string IMapSourceField.SourceField => SourceColumn?.Key;

                string IMapSourceField.TargetField => TargetField?.Key;

                bool IMapSourceField.UseAltMatchField => false;

                string IMapSourceField.AltMatchFieldType => null;

                string IMapSourceField.AltMatchField => null;

                public override string ToString()
                {
                    return (SourceColumn?.Value ?? "(None)") + " > " + (TargetField?.Value ?? "(None)");
                }
            }

            [DoNotAllowGridOpen]
            public class SqlImportMatchKey : IMapSourceMatchKey
            {
                [RequiredProperty]
                public RecordField TargetField { get; set; }

                string IMapSourceMatchKey.TargetField => TargetField?.Key;
                string IMapSourceMatchKey.TargetFieldLabel => TargetField?.Value;
                bool IMapSourceMatchKey.CaseSensitive => false;

                public override string ToString()
                {
                    return (TargetField?.Value ?? "(Empty)");
                }
            }
        }
    }
}