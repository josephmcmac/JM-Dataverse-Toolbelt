using JosephM.Core.Attributes;
using JosephM.Record.IService;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using System.Linq;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmRecordTypeMetadata : IRecordTypeMetadata
    {
        public string SchemaName { get; set; }
        private XrmService XrmService { get; set; }

        public XrmRecordTypeMetadata(string schemaName, XrmService xrmService)
        {
            SchemaName = schemaName;
            XrmService = xrmService;
        }

        public override string ToString()
        {
            return DisplayName ?? base.ToString();
        }

        public string DisplayName { get { return XrmService.GetEntityDisplayName(SchemaName); } }

        public bool Audit
        {
            get { return XrmService.IsEntityAuditOn(SchemaName); }
        }

        public string CollectionName { get { return XrmService.GetEntityCollectionName(SchemaName); } }
        public string PrimaryFieldSchemaName { get { return XrmService.GetPrimaryNameField(SchemaName); } }
        public string PrimaryKeyName { get { return XrmService.GetPrimaryKeyField(SchemaName); } }

        public bool Notes
        {
            get
            {
                return XrmService.GetLookupTargetEntity(Fields.annotation_.objectid, Entities.annotation).Contains(SchemaName);
            }
        }

        public bool Activities
        {
            get
            {
                return XrmService.GetLookupTargetEntity(Fields.activitypointer_.regardingobjectid, Entities.activitypointer).Split(',').Contains(SchemaName);
            }
        }

        public bool Connections
        {
            get { return XrmService.HasConnections(SchemaName); }
        }

        public bool MailMerge
        {
            get { return XrmService.HasMailMerge(SchemaName); }
        }

        public bool Queues
        {
            get { return XrmService.HasQueues(SchemaName); }
        }

        public string Description
        {
            get { return XrmService.GetDescription(SchemaName); }
        }

        public bool IsActivityType
        {
            get { return XrmService.GetEntityMetadata(SchemaName).IsActivity ?? false; }
        }

        public bool IsActivityParticipant
        {
            get { return XrmService.GetEntityMetadata(SchemaName).IsActivityParty ?? false; }
        }

        public bool Searchable { get { return XrmService.GetEntityMetadata(SchemaName).IsValidForAdvancedFind ?? false; } }
        public bool IsCustomType { get { return XrmService.GetEntityMetadata(SchemaName).IsCustomEntity ?? false; } }

        public string RecordTypeCode
        {
            get
            {
                var code = XrmService.GetEntityMetadata(SchemaName).ObjectTypeCode;
                return code.HasValue ? code.ToString() : null;
            }
        }
        [Key]
        public string MetadataId
        {
            get
            {
                var id = XrmService.GetEntityMetadata(SchemaName).MetadataId;
                return id != null ? id.ToString() : null;
            }
        }

        public bool HasOwner
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).OwnershipType == Microsoft.Xrm.Sdk.Metadata.OwnershipTypes.UserOwned;
            }
        }

        public string SchemaNameQualified
        {
            get
            {
                return SchemaName;
            }
        }

        public bool ChangeTracking
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).ChangeTrackingEnabled ?? false;
            }
        }

        public string EntitySetName
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).EntitySetName;
            }
        }

        public bool DocumentManagement
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsDocumentManagementEnabled ?? false;
            }
        }

        public bool QuickCreate
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsQuickCreateEnabled ?? false;
            }
        }

        public string PrimaryImage
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).PrimaryImageAttribute;
            }
        }

        public string Colour
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).EntityColor;
            }
        }

        public bool BusinessProcessEnabled
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsBusinessProcessEnabled ?? false;
            }
        }

        public string IconSmall
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IconSmallName;
            }
        }

        public string IconMedium
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IconMediumName;
            }
        }

        public string IconLarge
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IconLargeName;
            }
        }

        public string IconVector
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IconVectorName;
            }
        }

        public bool OneNote
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsOneNoteIntegrationEnabled ?? false;
            }
        }

        public bool AccessTeams
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).AutoCreateAccessTeams ?? false;
            }
        }

        public bool AutoRouteToOwner
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).AutoRouteToOwnerQueue ?? false;
            }
        }

        public bool KnowledgeManagement
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsKnowledgeManagementEnabled ?? false;
            }
        }

        public bool Slas
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsSLAEnabled ?? false;
            }
        }

        public bool DuplicateDetection
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsDuplicateDetectionEnabled?.Value ?? false;
            }
        }

        public bool Mobile
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsVisibleInMobile?.Value ?? false;
            }
        }

        public bool MobileClient
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsVisibleInMobileClient?.Value ?? false;
            }
        }

        public bool MobileClientReadOnly
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsReadOnlyInMobileClient?.Value ?? false;
            }
        }

        public bool MobileClientOffline
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsOfflineInMobileClient?.Value ?? false;
            }
        }

        public string MobileOfflineFilters
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).MobileOfflineFilters;
            }
        }

        public bool ReadingPane
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).IsReadingPaneEnabled ?? false;
            }
        }

        public bool HelpUrlEnabled
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).EntityHelpUrlEnabled ?? false;
            }
        }

        public string HelpUrl
        {
            get
            {
                return XrmService.GetEntityMetadata(SchemaName).EntityHelpUrl;
            }
        }
    }
}
