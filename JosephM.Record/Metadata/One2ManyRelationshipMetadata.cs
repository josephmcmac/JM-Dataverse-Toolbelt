namespace JosephM.Record.Metadata
{
    public class One2ManyRelationshipMetadata
    {
        public string ReferencedAttribute { get; set; }

        public string ReferencedEntity { get; set; }

        public string ReferencingAttribute { get; set; }

        public string ReferencingEntity { get; set; }

        public string SchemaName { get; set; }
    }
}