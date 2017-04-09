using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Core.FieldType;

namespace JosephM.Record.Metadata
{
    public interface IMetadata
    {
        string MetadataId { get; }
        string SchemaName { get; }
        string SchemaNameQualified { get; }
    }
}
