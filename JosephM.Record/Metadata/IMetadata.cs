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
