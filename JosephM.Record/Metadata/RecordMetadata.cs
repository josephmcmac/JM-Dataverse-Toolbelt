#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace JosephM.Record.Metadata
{
    public class RecordMetadata
    {
        public IEnumerable<ViewMetadata> Views { get; set; }
        public IEnumerable<FieldMetadata> Fields { get; set; }
        public string DisplayName { get; set; }
        public string SchemaName { get; set; }
        public string PrimaryKeyName { get; set; }
        public string DisplayCollectionName { get; set; }
        public string Description { get; set; }
        public bool Audit { get; set; }
        public bool IsActivityType { get; set; }
        public bool IsActivityParticipant { get; set; }
        public bool Notes { get; set; }
        public bool Activities { get; set; }
        public bool Connections { get; set; }
        public bool MailMerge { get; set; }
        public bool Queues { get; set; }

        public string PrimaryFieldSchemaName
        {
            get { return GetPrimaryFieldMetadata().SchemaName; }
        }

        public string PrimaryFieldDisplayName
        {
            get { return GetPrimaryFieldMetadata().DisplayName; }
        }

        public string PrimaryFieldDescription
        {
            get { return GetPrimaryFieldMetadata().Description; }
        }

        public int PrimaryFieldMaxLength
        {
            get { return GetPrimaryFieldMetadata().MaxLength; }
        }

        public bool PrimaryFieldIsMandatory
        {
            get { return GetPrimaryFieldMetadata().IsMandatory; }
        }

        public bool PrimaryFieldAudit
        {
            get { return GetPrimaryFieldMetadata().Audit; }
        }

        public StringFieldMetadata GetPrimaryFieldMetadata()
        {
            if (
                Fields.Where(f => f is StringFieldMetadata)
                    .Cast<StringFieldMetadata>().Any(f => f.IsPrimaryField))
                return Fields
                    .Where(f => f is StringFieldMetadata)
                    .Cast<StringFieldMetadata>().First(f => f.IsPrimaryField);
            throw new ArgumentNullException("There is no primary field defined for type " + SchemaName);
        }
    }
}