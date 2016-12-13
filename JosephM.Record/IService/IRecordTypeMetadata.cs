namespace JosephM.Record.IService
{
    public interface IRecordTypeMetadata
    {
        string SchemaName { get; }
        string DisplayName { get; }
        bool Audit { get; }
        string CollectionName { get; }
        string PrimaryFieldSchemaName { get; }
        string PrimaryKeyName { get; }

        string Description { get; }

        bool Notes { get; }

        bool Activities { get; }

        bool Connections { get; }

        bool MailMerge { get; }

        bool Queues { get; }

        bool IsActivityType { get; }

        bool IsActivityParticipant { get; }

        bool Searchable { get; }

        bool IsCustomType { get; }

        string RecordTypeCode { get; }

        string MetadataId { get; }

        bool HasOwner { get; }
    }
}
