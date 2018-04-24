using JosephM.Core.Attributes;
using JosephM.Record.Metadata;

namespace JosephM.CustomisationImporter.ImportMetadata
{
    [DisplayName("Picklist Field Options")]
    public class ImportPicklistFieldOptions : IMetadata
    {
        public ImportPicklistFieldOptions(PicklistFieldMetadata fieldMetadata)
        {
            FieldMetadata = fieldMetadata;
        }

        public PicklistFieldMetadata FieldMetadata { get; set; }

        public string MetadataId
        {
            get
            {
                return FieldMetadata.MetadataId;
            }
        }

        public string SchemaName
        {
            get
            {
                return FieldMetadata.SchemaName;
            }
        }

        public string SchemaNameQualified
        {
            get
            {
                return FieldMetadata.SchemaNameQualified;
            }
        }
    }
}
