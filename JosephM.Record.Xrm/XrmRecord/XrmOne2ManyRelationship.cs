using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Record.Metadata;
using JosephM.Xrm;
using Microsoft.Xrm.Sdk.Metadata;
using JosephM.Core.Attributes;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmOne2ManyRelationship : XrmConfigurationBase, IOne2ManyRelationshipMetadata
    {
        public string ReferencingEntity { get { return GetMetadata().ReferencingEntity; } }
        public string SchemaName { get; set; }
        public string ReferencedAttribute { get { return GetMetadata().ReferencedAttribute; } }
        public string ReferencedEntity { get; set; }
        public string ReferencingAttribute { get { return GetMetadata().ReferencingAttribute; } }

        public XrmOne2ManyRelationship(string name, string referencedType, XrmService xrmService)
            : base(xrmService)
        {
            ReferencedEntity = referencedType;
            SchemaName = name;
        }

        private OneToManyRelationshipMetadata GetMetadata()
        {
            return XrmService.GetOneToManyRelationship(ReferencedEntity, SchemaName);
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
