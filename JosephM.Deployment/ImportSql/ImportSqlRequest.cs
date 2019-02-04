using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Sql;
using JosephM.Xrm.Schema;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportSql
{
    [Instruction("This feature has been implemented for an OLEDB connection to a SQL Server instance so you must enter a valid OLEDB connection string\n\nThis example connection string is for a SQL Server database on the local machine using the current windows login\nProvider=sqloledb;Data Source=localhost;Initial Catalog=sourcedatabasename;Integrated Security=SSPI;")]
    [DisplayName("Import SQL")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Misc, true, 20)]
    [Group(Sections.SchedulingOptions, true, 20)]
    public class ImportSqlRequest : ServiceRequestBase, IValidatableObject
    {
        public ImportSqlRequest()
        {
            MatchRecordsByName = true;
        }

        [DisplayName("OLEDB Connection String")]
        [DisplayOrder(15)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [ConnectionFor(nameof(Mappings) + "." + nameof(SqlImportTableMapping.SourceTable), typeof(SqlConnectionString))]
        [ConnectionFor(nameof(Mappings) + "." + nameof(SqlImportTableMapping.Mappings) + "." + nameof(SqlImportTableMapping.SqlImportFieldMapping.SourceColumn), typeof(SqlConnectionString))]
        public string ConnectionString { get; set; }

        [Group(Sections.SchedulingOptions)]
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

        [Group(Sections.Misc)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(410)]
        [RequiredProperty]
        public bool MatchRecordsByName { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(415)]
        [RequiredProperty]
        public bool UpdateOnly { get; set; }

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
            public const string Misc = "Misc";
            public const string SchedulingOptions = "Scheduling Options";
        }

        [DoNotAllowGridOpen]
        [Group(Sections.Main, true, 10)]
        public class SqlImportTableMapping : IMapSpreadsheetImport
        {
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

            string IMapSpreadsheetImport.SourceType => SourceTable?.Key;
            string IMapSpreadsheetImport.TargetType => TargetType?.Key;
            string IMapSpreadsheetImport.TargetTypeLabel => TargetType?.Value;
            IEnumerable<IMapSpreadsheetMatchKey> IMapSpreadsheetImport.AltMatchKeys => AltMatchKeys;
            IEnumerable<IMapSpreadsheetColumn> IMapSpreadsheetImport.FieldMappings => Mappings;

            public override string ToString()
            {
                return (SourceTable?.Value ?? "(None)") + " > " + (TargetType?.Value ?? "(None)");
            }

            private static class Sections
            {
                public const string Main = "Main";
            }

            [DoNotAllowGridOpen]
            public class SqlImportFieldMapping : IMapSpreadsheetColumn
            {
                [RequiredProperty]
                public RecordField SourceColumn { get; set; }

                [RequiredProperty]
                public RecordField TargetField { get; set; }

                string IMapSpreadsheetColumn.SourceField => SourceColumn?.Key;

                string IMapSpreadsheetColumn.TargetField => TargetField?.Key;

                public override string ToString()
                {
                    return (SourceColumn?.Value ?? "(None)") + " > " + (TargetField?.Value ?? "(None)");
                }
            }

            [DoNotAllowGridOpen]
            public class SqlImportMatchKey : IMapSpreadsheetMatchKey
            {
                [RequiredProperty]
                public RecordField TargetField { get; set; }

                string IMapSpreadsheetMatchKey.TargetField => TargetField?.Key;
                string IMapSpreadsheetMatchKey.TargetFieldLabel => TargetField?.Value;

                public override string ToString()
                {
                    return (TargetField?.Value ?? "(Empty)");
                }
            }
        }
    }
}