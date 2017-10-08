using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Core.FieldType;
using JosephM.Core.Attributes;

namespace JosephM.Record.Metadata
{
    public interface IMetadata
    {
        [Hidden]
        string MetadataId { get; }
        [DisplayOrder(5)]
        string SchemaName { get; }
        [Hidden]
        string SchemaNameQualified { get; }
    }
}
