using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Core.FieldType;
using JosephM.Core.Attributes;
using JosephM.Record.Attributes;

namespace JosephM.Record.Metadata
{
    public interface IMetadata
    {
        [Key]
        [Hidden]
        string MetadataId { get; }
        [DisplayOrder(5)]

        [QuickFind]
        string SchemaName { get; }
        [Hidden]
        string SchemaNameQualified { get; }
    }
}
