namespace JosephM.Record.Metadata
{
    public class One2ManyRelationshipMetadata : IOne2ManyRelationshipMetadata
    {
        public string ReferencedAttribute { get; set; }

        public string ReferencedEntity { get; set; }

        public string ReferencingAttribute { get; set; }

        public string ReferencingEntity { get; set; }

        public string SchemaName { get; set; }
        public int DisplayOrder { get; set; }
        public bool DisplayRelated { get; set; }
        public bool IsCustomLabel { get; set; }
        public string GetRelationshipLabel { get; set; }
        public string MetadataId { get; set; }
        public string DeleteCascadeConfiguration { get; set; }
    }
}