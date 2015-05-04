using Microsoft.Xrm.Sdk.Metadata;
using JosephM.ObjectMapping;
using JosephM.Record.Metadata;

namespace JosephM.Record.Xrm.Mappers
{
    internal class ManyToManyRelationshipTypeMapper :
        ClassMapperFor<ManyToManyRelationshipMetadata, Many2ManyRelationshipMetadata>
    {
    }
}