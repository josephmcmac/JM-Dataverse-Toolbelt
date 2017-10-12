using System;
using JosephM.Record.IService;
using JosephM.Xrm;
using JosephM.Record.Attributes;
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
    }
}
