using JosephM.Core.Attributes;
using JosephM.Record.Metadata;
using JosephM.Xrm;
using Microsoft.Xrm.Sdk.Metadata;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmManyToManyRelationshipMetadata : XrmConfigurationBase, IMany2ManyRelationshipMetadata
    {
        public string Entity1IntersectAttribute
        {
            get { return GetMetadata().Entity1IntersectAttribute; }
        }

        public string RecordType1
        {
            get { return GetMetadata().Entity1LogicalName; }
        }

        public string Entity2IntersectAttribute
        {
            get { return GetMetadata().Entity2IntersectAttribute; }
        }

        public string RecordType2
        {
            get { return GetMetadata().Entity2LogicalName; }
        }

        public string IntersectEntityName
        {
            get { return GetMetadata().IntersectEntityName; }
        }

        public string SchemaName { get; set; }

        public bool RecordType1DisplayRelated { get { return IsManyToManyDisplayRelated(false); } }
        public bool RecordType2DisplayRelated { get { return IsManyToManyDisplayRelated(true); } }
        public bool RecordType1UseCustomLabel { get { return IsManyToManyCustomLabel(false); } }
        public bool RecordType2UseCustomLabel { get { return IsManyToManyCustomLabel(true); } }
        public string RecordType1CustomLabel { get { return GetLabel(false); } }
        public string RecordType2CustomLabel { get { return GetLabel(true); } }
        public int RecordType1DisplayOrder { get { return GetManyToManyDisplayOrder(false); } }
        public int RecordType2DisplayOrder { get { return GetManyToManyDisplayOrder(true); } }
        public bool IsCustomRelationship { get { return GetMetadata().IsCustomRelationship ?? false; } }
        [Key]
        public string MetadataId
        {
            get
            {
                var id = GetMetadata().MetadataId;
                return id != null ? id.ToString() : null;
            }
        }

        public string SchemaNameQualified
        {
            get
            {
                return SchemaName;
            }
        }

        public XrmManyToManyRelationshipMetadata(string name, XrmService xrmService, string recordType)
            : base(xrmService)
        {
            _recordTypeForLoading = recordType;
            SchemaName = name;
        }

        public XrmManyToManyRelationshipMetadata(ManyToManyRelationshipMetadata sdkMetadata, XrmService xrmService)
            : base(xrmService)
        {
            SchemaName = sdkMetadata.SchemaName;
            _sdkMetadata = sdkMetadata;
        }

        private string _recordTypeForLoading;
        private ManyToManyRelationshipMetadata _sdkMetadata;
        private ManyToManyRelationshipMetadata GetMetadata()
        {
            if (_sdkMetadata == null)
            {
                _sdkMetadata = XrmService.GetManyToManyRelationship(_recordTypeForLoading, SchemaName); ;
            }
            return _sdkMetadata;
        }

        public bool IsManyToManyDisplayRelated(bool forRecordType2)
        {
            var relationship = GetMetadata();
            var menuConfiguration = GetAssociatedMenuConfiguration(forRecordType2, relationship);
            return GetIsDisplayRelated(menuConfiguration);
        }

        private static AssociatedMenuConfiguration GetAssociatedMenuConfiguration(bool forRecordType2, ManyToManyRelationshipMetadata relationship)
        {
            var menuConfiguration =
                forRecordType2
                    ? relationship.Entity2AssociatedMenuConfiguration
                    : relationship.Entity1AssociatedMenuConfiguration;
            return menuConfiguration;
        }

        public bool IsManyToManyCustomLabel(bool forRecordType2)
        {
            var relationship = GetMetadata();
            var menuConfiguration = GetAssociatedMenuConfiguration(forRecordType2, relationship);
            return GetIsCustomLabel(menuConfiguration);
        }

        public int ManyToManyDisplayOrder(bool forRecordType2)
        {
            var relationship = GetMetadata();
            var menuConfiguration = GetAssociatedMenuConfiguration(forRecordType2, relationship);
            return GetDisplayOrder(menuConfiguration);
        }

        private string GetLabel(bool forRecordType2)
        {
            var relationship = GetMetadata();
            if (IsManyToManyCustomLabel(forRecordType2))
            {
                var menuConfiguration = GetAssociatedMenuConfiguration(forRecordType2, relationship);
                return GetCustomLabel(menuConfiguration);
            }
            return XrmService.GetEntityCollectionName(forRecordType2 ? relationship.Entity2LogicalName : relationship.Entity1LogicalName);
        }

        public int GetManyToManyDisplayOrder(bool forRecordType2)
        {
            var relationship = GetMetadata();
            var menuConfiguration = GetAssociatedMenuConfiguration(forRecordType2, relationship);
            return GetDisplayOrder(menuConfiguration);
        }

        public string PicklistDisplay
        {
            get { return XrmService.GetEntityDisplayName(RecordType1) + " " + XrmService.GetEntityCollectionName(RecordType1) + " (N:N Association)"; }
        }
    }
}
