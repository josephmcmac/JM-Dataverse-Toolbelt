namespace JosephM.Record.Metadata
{
    public interface IMany2ManyRelationshipMetadata : IMetadata
    {
        string Entity1IntersectAttribute { get; }

        string RecordType1 { get; }

        string Entity2IntersectAttribute { get; }

        string RecordType2 { get; }

        string IntersectEntityName { get; }

        bool RecordType1DisplayRelated { get; }

        bool RecordType2DisplayRelated { get; }

        bool RecordType1UseCustomLabel { get; }

        bool RecordType2UseCustomLabel { get; }

        string RecordType1CustomLabel { get; }

        string RecordType2CustomLabel { get; }

        int RecordType1DisplayOrder { get; }

        int RecordType2DisplayOrder { get; }

        bool IsCustomRelationship { get; }

        string PicklistDisplay { get; }
    }
}