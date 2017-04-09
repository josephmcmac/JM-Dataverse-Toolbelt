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
    [DisplayName("Record Type Views")]
    public class ImportViews : IMetadata
    {
        public ImportViews(RecordMetadata typeMetadata)
        {
            TypeMetadata = typeMetadata;
        }

        public RecordMetadata TypeMetadata { get; set; }

        public string MetadataId
        {
            get
            {
                return TypeMetadata.MetadataId;
            }
        }

        public string SchemaName
        {
            get
            {
                return TypeMetadata.SchemaName;
            }
        }

        public string SchemaNameQualified
        {
            get
            {
                return TypeMetadata.SchemaNameQualified;
            }
        }
    }
}
