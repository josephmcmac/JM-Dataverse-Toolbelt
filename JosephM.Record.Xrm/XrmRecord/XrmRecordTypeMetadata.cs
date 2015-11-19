using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Record.IService;
using JosephM.Xrm;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmRecordTypeMetadata : IRecordTypeMetadata
    {
        private string RecordType { get; set; }
        private XrmService XrmService { get; set; }

        public XrmRecordTypeMetadata(string recordType, XrmService xrmService)
        {
            RecordType = recordType;
            XrmService = xrmService;
        }

        public string DisplayName { get { return XrmService.GetEntityDisplayName(RecordType); } }

        public bool Audit
        {
            get { return XrmService.IsEntityAuditOn(RecordType); }
        }

        public string CollectionName { get { return XrmService.GetEntityCollectionName(RecordType); } }
        public string PrimaryFieldSchemaName { get { return XrmService.GetPrimaryNameField(RecordType); }}
        public string PrimaryKeyName { get { return XrmService.GetPrimaryKeyField(RecordType); } }

        public bool Notes
        {
            get { return XrmService.HasNotes(RecordType); }
        }

        public bool Activities
        {
            get { return XrmService.HasActivities(RecordType); }
        }

        public bool Connections
        {
            get { return XrmService.HasConnections(RecordType); }
        }

        public bool MailMerge
        {
            get { return XrmService.HasMailMerge(RecordType); }
        }

        public bool Queues
        {
            get { return XrmService.HasQueues(RecordType); }
        }

        public string Description
        {
            get { return XrmService.GetDescription(RecordType); }
        }

        public bool IsActivityType
        {
            get { return XrmService.GetEntityMetadata(RecordType).IsActivity ?? false; }
        }

        public bool IsActivityParticipant
        {
            get { return XrmService.GetEntityMetadata(RecordType).IsActivityParty ?? false; }
        }

        public bool Searchable { get { return XrmService.GetEntityMetadata(RecordType).IsValidForAdvancedFind ?? false; } }
        public bool IsCustomType { get { return XrmService.GetEntityMetadata(RecordType).IsCustomEntity ?? false; } }

        public string RecordTypeCode
        {
            get
            {
                var code = XrmService.GetEntityMetadata(RecordType).ObjectTypeCode;
                return code.HasValue ? code.ToString() : null;
            }
        }

        public string MetadataId
        {
            get
            {
                var id = XrmService.GetEntityMetadata(RecordType).MetadataId;
                return id?.ToString();
            }
        }
    }
}
