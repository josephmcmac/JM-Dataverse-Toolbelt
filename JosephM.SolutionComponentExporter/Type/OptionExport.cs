using JosephM.Core.Attributes;
using JosephM.Xrm.MetadataImportExport;

namespace JosephM.SolutionComponentExporter.Type
{
    public class OptionExport
    {
        public OptionExport(string recordTypeLabel, string recordTypeSchemaName, string fieldLabel,
            string fieldSchemaName, string key, string value
            , bool isSharedOptionSet, string schemaName, string optionSetName)
        {
            RecordTypeLabel = recordTypeLabel;
            RecordTypeSchemaName = recordTypeSchemaName;
            FieldLabel = fieldLabel;
            FieldSchemaName = fieldSchemaName;
            Key = key;
            Value = value;
            IsSharedOptionSet = isSharedOptionSet;
            SchemaName = schemaName;
            OptionSetName = optionSetName;
        }

        [DisplayName(Headings.OptionSets.SchemaName)]
        public string SchemaName { get; set; }

        [DisplayName(Headings.OptionSets.OptionSetName)]
        public string OptionSetName { get; set; }

        [DisplayName(Headings.OptionSets.IsSharedOptionSet)]
        public bool IsSharedOptionSet { get; set; }

        [DisplayName(Headings.OptionSets.Index)]
        public string Key { get; set; }

        [DisplayName(Headings.OptionSets.Label)]
        public string Value { get; set; }

        public string FieldLabel { get; set; }
        public string FieldSchemaName { get; set; }
        public string RecordTypeLabel { get; set; }
        public string RecordTypeSchemaName { get; set; }
    }
}