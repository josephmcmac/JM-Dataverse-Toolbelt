using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
