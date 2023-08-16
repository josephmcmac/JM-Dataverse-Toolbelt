using System;

namespace JosephM.Record.Metadata
{
    public class Many2ManyRelationshipMetadata : IMany2ManyRelationshipMetadata
    {
        public Many2ManyRelationshipMetadata()
        {
            RecordType1DisplayRelated = true;
            RecordType2DisplayRelated = true;
        }

        public string IntersectEntityName { get; set; }
        public string SchemaName { get; set; }

        public string Entity1IntersectAttribute { get; set; }
        public string RecordType1 { get; set; }
        public string Entity2IntersectAttribute { get; set; }

        public string RecordType2 { get; set; }

        public bool RecordType1DisplayRelated { get; set; }

        public bool RecordType2DisplayRelated { get; set; }

        public bool RecordType1UseCustomLabel { get; set; }

        public bool RecordType2UseCustomLabel { get; set; }

        public string RecordType1CustomLabel { get; set; }

        public string RecordType2CustomLabel { get; set; }

        public int RecordType1DisplayOrder { get; set; }

        public int RecordType2DisplayOrder { get; set; }

        public bool IsCustomRelationship { get; set; }
        public string MetadataId { get; set; }
        public string ReferencingRecordType { get; set; }


        public string SchemaNameQualified
        {
            get
            {
                return SchemaName;
            }
        }

        public string PicklistDisplay => SchemaName;
    }
}