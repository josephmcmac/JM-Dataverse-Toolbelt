using JosephM.Core.Attributes;
using JosephM.Record.Metadata;
using JosephM.Xrm;
using Microsoft.Xrm.Sdk.Metadata;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmMany2OneRelationship : XrmConfigurationBase, IOne2ManyRelationshipMetadata
    {
        public string ReferencingEntity { get; set; } 
        public string SchemaName { get; set; }
        public string ReferencedAttribute { get { return GetMetadata().ReferencedAttribute; } }
        public string ReferencedEntity { get { return GetMetadata().ReferencedEntity; } }
        public string ReferencingAttribute { get { return GetMetadata().ReferencingAttribute; } }

        public XrmMany2OneRelationship(string name, string referencingType, XrmService xrmService)
            : base(xrmService)
        {
            ReferencingEntity = referencingType;
            SchemaName = name;
        }

        private OneToManyRelationshipMetadata GetMetadata()
        {
            return XrmService.GetManyToOneRelationship(ReferencingEntity, SchemaName);
        }

        public int DisplayOrder
        {
            get { return GetDisplayOrder(GetMetadata().AssociatedMenuConfiguration); }
        }

        public bool DisplayRelated
        {
            get { return GetIsDisplayRelated(GetMetadata().AssociatedMenuConfiguration); }
        }

        public bool IsCustomLabel
        {
            get { return GetIsCustomLabel(GetMetadata().AssociatedMenuConfiguration); }
        }

        public string GetRelationshipLabel
        {

            get
            {
                var mt = GetMetadata(); 
                return IsCustomLabel ? GetCustomLabel(mt.AssociatedMenuConfiguration) : XrmService.GetEntityCollectionName(mt.ReferencingEntity);
            }
        }
        [Key]
        public string MetadataId
        {
            get
            {
                var id = GetMetadata().MetadataId;
                return id != null ? id.ToString() : null;
            }
        }

        public string DeleteCascadeConfiguration
        {
            get
            {
                var id = GetMetadata().CascadeConfiguration.Delete;
                return id != null ? id.ToString() : null;
            }
        }
    }
}
