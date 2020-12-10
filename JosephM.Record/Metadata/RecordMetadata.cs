using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.IService;

namespace JosephM.Record.Metadata
{
    public class RecordMetadata : IRecordTypeMetadata
    {
        public RecordMetadata()
        {
            Searchable = true;
        }

        public override string ToString()
        {
            return DisplayName ?? base.ToString();
        }

        public IEnumerable<ViewMetadata> Views { get; set; }
        public IEnumerable<FieldMetadata> Fields { get; set; }
        public string DisplayName { get; set; }
        public string SchemaName { get; set; }

        public string PrimaryKeyName
        {
            get
            {
                var mt = Fields.Where(m => m.IsPrimaryKey);
                return mt.Any() ? mt.First().SchemaName : null;
            }
        }

        public string CollectionName { get; set; }
        public string Description { get; set; }
        public bool Audit { get; set; }
        public bool IsActivityType { get; set; }
        public bool IsActivityParticipant { get; set; }
        public bool Notes { get; set; }
        public bool Activities { get; set; }
        public bool Connections { get; set; }
        public bool MailMerge { get; set; }
        public bool Queues { get; set; }
        public bool Searchable { get; set; }
        public bool IsCustomType { get; set; }
        public virtual string RecordTypeCode { get { return SchemaName; } }
        public string MetadataId { get; set; }

        public virtual string PrimaryFieldSchemaName
        {
            get { return HasPrimaryFieldMetadata() ? GetPrimaryFieldMetadata().SchemaName : null; }
        }

        public virtual string PrimaryFieldDisplayName
        {
            get { return GetPrimaryFieldMetadata().DisplayName; }
        }

        public virtual string PrimaryFieldDescription
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

        public bool HasOwner
        {
            get
            {
                return false;
            }
        }

        public bool HasPrimaryFieldMetadata()
        {
            return Fields.Where(f => f is StringFieldMetadata).Cast<StringFieldMetadata>().Any(f => f.IsPrimaryField);
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

        public string SchemaNameQualified
        {
            get
            {
                return SchemaName ?? DisplayName;
            }
        }

        public bool ChangeTracking
        {
            get
            {
                return false;
            }
        }

        public string EntitySetName
        {
            get { return SchemaName; }
        }

        public bool DocumentManagement
        {
            get
            {
                return false;
            }
        }

        public bool QuickCreate
        {
            get
            {
                return false;
            }
        }

        public string PrimaryImage => null;

        public string Colour => null;

        public bool BusinessProcessEnabled => false;

        public string IconSmall => null;

        public string IconMedium => null;

        public string IconLarge => null;

        public string IconVector => null;

        public bool OneNote => false;

        public bool AccessTeams => false;

        public bool AutoRouteToOwner => false;

        public bool KnowledgeManagement => false;

        public bool Slas => false;

        public bool DuplicateDetection => false;

        public bool Mobile => false;

        public bool MobileClient => false;

        public bool MobileClientReadOnly => false;

        public bool MobileClientOffline => false;

        public string MobileOfflineFilters => null;

        public bool ReadingPane => false;

        public bool HelpUrlEnabled => false;

        public string HelpUrl => null;

        public bool AutoAddToQueue => false;
    }
}