namespace JosephM.Record.Metadata
{
    public interface IOne2ManyRelationshipMetadata
    {
        string ReferencedAttribute { get; }

        string ReferencedEntity { get; }

        string ReferencingAttribute { get; }

        string ReferencingEntity { get; }

        string SchemaName { get; }

        int DisplayOrder { get; }

        bool DisplayRelated { get; }

        bool IsCustomLabel { get; }

        string GetRelationshipLabel { get; }
    }
}