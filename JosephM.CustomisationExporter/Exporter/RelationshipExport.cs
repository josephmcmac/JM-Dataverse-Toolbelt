using JosephM.Core.Attributes;
using JosephM.Xrm.MetadataImportExport;

namespace JosephM.CustomisationExporter.Exporter
{
    public class RelationshipExport
    {
        public RelationshipExport(string relationshipSchemaName, string recordTypeFrom, string recordTypeTo,
            bool isCustomRelationship
            , bool displayRelated, bool displayRelated2, string recordType1ReferenceField,
            string recordType2ReferenceField, RelationshipType type, bool recordType1UseCustomLabel,
            bool recordType2UseCustomLabel, string recordType1CustomLabel, string recordType2CustomLabel,
            int recordType1DisplayOrder, int recordType2DisplayOrder, string metadataId, string cascadeDeleteConfiguration)
        {
            RelationshipSchemaName = relationshipSchemaName;
            RecordTypeFrom = recordTypeFrom;
            RecordTypeTo = recordTypeTo;
            IsCustomRelationship = isCustomRelationship;
            DisplayRelated = displayRelated;
            DisplayRelated2 = displayRelated2;
            RecordType1ReferenceField = recordType1ReferenceField;
            RecordType2ReferenceField = recordType2ReferenceField;
            Type = type;
            RecordType1UseCustomLabel = recordType1UseCustomLabel;
            RecordType2UseCustomLabel = recordType2UseCustomLabel;
            RecordType1CustomLabel = recordType1CustomLabel;
            RecordType2CustomLabel = recordType2CustomLabel;
            RecordType1DisplayOrder = recordType1DisplayOrder;
            RecordType2DisplayOrder = recordType2DisplayOrder;
            MetadataId = metadataId;
            CascadeDeleteConfiguration = cascadeDeleteConfiguration;
        }

        [DisplayName(Headings.Relationships.RelationshipName)]
        public string RelationshipSchemaName { get; set; }

        [DisplayName(Headings.Relationships.RecordType1)]
        public string RecordTypeFrom { get; set; }

        [DisplayName(Headings.Relationships.RecordType2)]
        public string RecordTypeTo { get; set; }

        [DisplayName(Headings.Relationships.RecordType1DisplayRelated)]
        public bool DisplayRelated { get; set; }

        [DisplayName(Headings.Relationships.RecordType2DisplayRelated)]
        public bool DisplayRelated2 { get; set; }

        [DisplayName(Headings.Relationships.RecordType1UseCustomLabel)]
        public bool RecordType1UseCustomLabel { get; set; }

        [DisplayName(Headings.Relationships.RecordType2UseCustomLabel)]
        public bool RecordType2UseCustomLabel { get; set; }

        [DisplayName(Headings.Relationships.RecordType1CustomLabel)]
        public string RecordType1CustomLabel { get; set; }

        [DisplayName(Headings.Relationships.RecordType2CustomLabel)]
        public string RecordType2CustomLabel { get; set; }

        [DisplayName(Headings.Relationships.RecordType1DisplayOrder)]
        public int RecordType1DisplayOrder { get; set; }

        [DisplayName(Headings.Relationships.RecordType2DisplayOrder)]
        public int RecordType2DisplayOrder { get; set; }

        public bool IsCustomRelationship { get; set; }
        public string RecordType1ReferenceField { get; set; }
        public string RecordType2ReferenceField { get; set; }
        public RelationshipType Type { get; set; }

        public string MetadataId { get; set; }

        public string CascadeDeleteConfiguration { get; set; }

        public enum RelationshipType
        {
            OneToMany,
            ManyToMany
        }
    }
}