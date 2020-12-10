using JosephM.Core.Attributes;
using JosephM.Record.Attributes;
using JosephM.Record.Metadata;

namespace JosephM.Record.IService
{
    public interface IRecordTypeMetadata : IMetadata
    {
        //the hiddens are used for the add multiple grid view
        [DisplayOrder(10)]
        [QuickFind]
        string DisplayName { get; }
        [Hidden]
        bool Audit { get; }
        [DisplayOrder(20)]
        [QuickFind]
        string CollectionName { get; }
        [Hidden]
        string PrimaryFieldSchemaName { get; }
        [Hidden]
        string PrimaryKeyName { get; }
        [DisplayOrder(40)]
        [GridWidth(400)]
        [QuickFind]
        string Description { get; }
        [Hidden]
        bool Notes { get; }
        [Hidden]
        bool Activities { get; }
        [Hidden]
        bool Connections { get; }

        [Hidden]
        bool MailMerge { get; }
        [Hidden]
        bool Queues { get; }
        [Hidden]
        bool IsActivityType { get; }
        [Hidden]
        bool IsActivityParticipant { get; }
        [Hidden]
        bool Searchable { get; }
        [DisplayOrder(30)]
        [GridWidth(100)]
        bool IsCustomType { get; }
        [Hidden]
        string RecordTypeCode { get; }
        [Hidden]
        bool HasOwner { get; }
        [Hidden]
        bool ChangeTracking { get; }
        [Hidden]
        string EntitySetName { get; }
        [Hidden]
        bool DocumentManagement { get; }
        [Hidden]
        bool QuickCreate { get; }
        [Hidden]
        string PrimaryImage { get; }
        [Hidden]
        string Colour { get; }
        [Hidden]
        bool BusinessProcessEnabled { get; }
        [Hidden]
        string IconSmall { get; }
        [Hidden]
        string IconMedium { get; }
        [Hidden]
        string IconLarge { get; }
        [Hidden]
        string IconVector { get; }
        [Hidden]
        bool OneNote { get; }
        [Hidden]
        bool AccessTeams { get; }
        [Hidden]
        bool AutoRouteToOwner { get; }
        [Hidden]
        bool KnowledgeManagement { get; }
        [Hidden]
        bool Slas { get; }
        [Hidden]
        bool DuplicateDetection { get; }
        [Hidden]
        bool Mobile { get; }
        [Hidden]
        bool MobileClient { get; }
        [Hidden]
        bool MobileClientReadOnly { get; }
        [Hidden]
        bool MobileClientOffline { get; }
        [Hidden]
        string MobileOfflineFilters { get; }
        [Hidden]
        bool ReadingPane { get; }
        [Hidden]
        bool HelpUrlEnabled { get; }
        [Hidden]
        string HelpUrl { get; }
        [Hidden]
        bool AutoAddToQueue { get; }
    }
}
