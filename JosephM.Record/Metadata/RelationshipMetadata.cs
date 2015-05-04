namespace JosephM.Record.Metadata
{
    public class RelationshipMetadata
    {
        public string SchemaName { get; set; }

        public string RecordType1 { get; set; }

        public string RecordType2 { get; set; }

        public bool RecordType1DisplayRelated { get; set; }

        public bool RecordType2DisplayRelated { get; set; }

        public bool RecordType1UseCustomLabel { get; set; }

        public bool RecordType2UseCustomLabel { get; set; }

        public string RecordType1CustomLabel { get; set; }

        public string RecordType2CustomLabel { get; set; }

        public int RecordType1DisplayOrder { get; set; }

        public int RecordType2DisplayOrder { get; set; }
    }
}