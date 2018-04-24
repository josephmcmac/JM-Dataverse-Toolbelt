using System.Net.Mail;

namespace JosephM.Xrm.MetadataImportExport
{
    public static class Headings
    {
        public static class Fields
        {
            public const string Ignore = "Ignore";
            public const string RecordTypeSchemaName = "Record Type Schema Name";
            public const string SchemaName = "Schema Name";
            public const string DisplayName = "Display Name";
            public const string FieldType = "Field Type";
            public const string Description = "Description";
            public const string IsPrimaryField = "Is Primary Field";
            public const string IsMandatory = "Is Mandatory";
            public const string Audit = "Audit";
            public const string Searchable = "Searchable";
            public const string ViewOrder = "View Order";
            public const string ViewWidth = "View Width";
            public const string ReferencedRecordType = "Referenced Record Type";
            public const string DisplayInRelated = "Display In Related";
            public const string MaxLength = "Max Length";
            public const string TextFormat = "Text Format";
            public const string DateBehaviour = "Date Behaviour";
            public const string IntegerFormat = "Integer Format";
            public const string IncludeTime = "Include Time";
            public const string Minimum = "Minimum";
            public const string Maximum = "Maximum";
            public const string PicklistOptions = "Picklist Options";
            public const string DecimalPrecision = "Decimal Precision";
            public const string IsMultiSelect = "Is Multi Select";
        }

        public static class RecordTypes
        {
            public const string Ignore = "Ignore";
            public const string DisplayName = "Display Name";
            public const string SchemaName = "Schema Name";
            public const string DisplayCollectionName = "Display Collection Name";
            public const string Description = "Description";
            public const string Audit = "Audit";
            public const string IsActivityType = "Is Activity Type";
            public const string Notes = "Notes";
            public const string Activities = "Activities";
            public const string Connections = "Connections";
            public const string MailMerge = "Mail Merge";
            public const string Queues = "Queues";
        }

        public static class Relationships
        {
            public const string Ignore = "Ignore";
            public const string RelationshipName = "Relationship Name";
            public const string RecordType1 = "Record Type 1";
            public const string RecordType2 = "Record Type 2";
            public const string RecordType1DisplayRelated = "Record Type 1 Display Related";
            public const string RecordType2DisplayRelated = "Record Type 2 Display Related";
            public const string RecordType1UseCustomLabel = "Record Type 1 Use Custom Label";
            public const string RecordType2UseCustomLabel = "Record Type 2 Use Custom Label";
            public const string RecordType1CustomLabel = "Record Type 1 Custom Label";
            public const string RecordType2CustomLabel = "Record Type 2 Custom Label";
            public const string RecordType1DisplayOrder = "Record Type 1 Display Order";
            public const string RecordType2DisplayOrder = "Record Type 2 Display Order";
        }

        public static class OptionSets
        {
            public const string Ignore = "Ignore";
            public const string SchemaName = "Schema Name";
            public const string OptionSetName = "Option Set Name";
            public const string IsSharedOptionSet = "Is Shared Option Set";
            public const string Index = "Index";
            public const string Label = "Label";
        }
    }
}
