using JosephM.Core.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.Mappers;
using JosephM.Xrm;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmFieldMetadata : XrmConfigurationBase, IFieldMetadata
    {
        private string FieldName { get; set; }
        public string RecordType { get; set; }

        public XrmFieldMetadata(string fieldName, string recordType, XrmService xrmService)
            : base(xrmService)
        {
            FieldName = fieldName;
            RecordType = recordType;
        }

        public string SchemaName
        {
            get { return FieldName; }
        }

        public RecordFieldType FieldType
        {
            get
            {
                return XrmService.GetFieldMetadata(FieldName, RecordType) is MultiSelectPicklistAttributeMetadata
                ? RecordFieldType.Picklist
                : new FieldTypeMapper().Map(XrmService.GetFieldType(FieldName, RecordType));
            }
        }

        public bool IsMandatory
        {
            get { return XrmService.IsMandatory(FieldName, RecordType); }
        }

        public string DisplayName
        {
            get { return XrmService.GetFieldLabel(FieldName, RecordType); }
        }

        public int MaxLength
        {
            get
            {
                return
                    XrmService.IsString(FieldName, RecordType)
                        ? XrmService.GetMaxLength(FieldName, RecordType)
                        : 0;
            }
        }

        public bool IsPrimaryKey
        {
            get { return XrmService.GetPrimaryKeyField(RecordType) == FieldName; }
        }

        public string Description
        {
            get { return XrmService.GetFieldDescription(FieldName, RecordType); }
        }

        public bool Audit
        {
            get { return XrmService.IsFieldAuditOn(FieldName, RecordType); }
        }

        public bool Searchable
        {
            get { return XrmService.IsFieldSearchable(FieldName, RecordType); }
        }

        public string DateBehaviour
        {
            get
            {
                if (FieldType != RecordFieldType.Date)
                    return "N/A";
                var format = ((DateTimeAttributeMetadata)XrmService.GetFieldMetadata(FieldName, RecordType)).DateTimeBehavior;

                if(format == null)
                    return "UserLocal";
                return format.Value;
            }
        }

        public bool IncludeTime
        {
            get
            {
                return FieldType == RecordFieldType.Date && XrmService.IsDateIncludeTime(FieldName, RecordType);
            }
        }

        public TextFormat TextFormat
        {
            get
            {
                if (FieldType != RecordFieldType.String)
                    return TextFormat.Text;
                var format = XrmService.GetTextFormat(FieldName, RecordType);
                return
                    format == null
                        ? TextFormat.Text
                        : new StringFormatMapper().Map((StringFormat)format);
            }
        }

        public int DecimalPrecision
        {
            get
            {
                return FieldType == RecordFieldType.Decimal || FieldType == RecordFieldType.Double
                    ? XrmService.GetPrecision(FieldName, RecordType)
                    : 0;
            }
        }

        public decimal MinValue
        {
            get
            {
                switch (FieldType)
                {
                    case RecordFieldType.Decimal: return XrmService.GetMinDecimalValue(FieldName, RecordType) ?? Decimal.MinValue;
                    case RecordFieldType.Double: return Convert.ToDecimal(XrmService.GetMinDoubleValue(FieldName, RecordType) ?? double.MinValue);
                    case RecordFieldType.Integer: return XrmService.GetMinIntValue(FieldName, RecordType) ?? int.MinValue;
                    case RecordFieldType.Money: return Convert.ToDecimal(XrmService.GetMinMoneyValue(FieldName, RecordType) ?? double.MinValue);
                }
                return 0;
            }
        }

        public decimal MaxValue
        {
            get
            {
                switch (FieldType)
                {
                    case RecordFieldType.Decimal: return XrmService.GetMaxDecimalValue(FieldName, RecordType) ?? Decimal.MaxValue;
                    case RecordFieldType.Double: return Convert.ToDecimal(XrmService.GetMaxDoubleValue(FieldName, RecordType) ?? double.MaxValue);
                    case RecordFieldType.Integer: return XrmService.GetMaxIntValue(FieldName, RecordType) ?? int.MaxValue;
                    case RecordFieldType.Money: return Convert.ToDecimal(XrmService.GetMaxMoneyValue(FieldName, RecordType) ?? double.MaxValue);
                }
                return 0;
            }
        }

        public IntegerType IntegerFormat
        {
            get
            {
                return FieldType == RecordFieldType.Integer
                  ? new IntegerTypeMapper().Map(XrmService.GetIntegerFormat(FieldName, RecordType))
                  : IntegerType.None;
            }
        }

        public bool IsNonNullable { get { return false; } }

        public bool IsSharedPicklist
        {
            get
            {
                return FieldType == RecordFieldType.Picklist && (((EnumAttributeMetadata)XrmService.GetFieldMetadata(FieldName, RecordType)).OptionSet.IsGlobal ?? false);
            }
        }

        public string PicklistName
        {
            get
            {
                if (!IsSharedPicklist)
                    return null;
                var name = ((EnumAttributeMetadata)XrmService.GetFieldMetadata(FieldName, RecordType)).OptionSet.Name;
                return XrmService.GetSharedPicklistDisplayName(name);
            }
        }

        public bool IsCustomField
        {
            get { return XrmService.GetFieldMetadata(FieldName, RecordType).IsCustomAttribute ?? false; }
        }

        public bool Readable
        {
            get { return XrmService.IsReadable(FieldName, RecordType); }
        }

        public bool Writeable
        {
            get
            {
                return XrmService.IsWritable(FieldName, RecordType)
                    && FieldType != RecordFieldType.ManagedProperty;
            }
        }

        public bool Createable
        {
            get
            {
                return XrmService.IsCreateable(FieldName, RecordType)
                    && FieldType != RecordFieldType.ManagedProperty;
            }
        }

        public bool IsDisplayRelated
        {
            get
            {
                if (!XrmService.IsLookup(FieldName, RecordType))
                    return false;
                var relationships = XrmService.GetEntityManyToOneRelationships(RecordType);
                var relationshipMatches = relationships.Where(r => r.ReferencingAttribute == FieldName && r.ReferencingEntity == RecordType).ToArray();
                if (!relationshipMatches.Any())
                    return false;
                return relationshipMatches.Any(r => r.AssociatedMenuConfiguration != null && GetIsDisplayRelated(r.AssociatedMenuConfiguration));
            }
        }

        [Key]
        public string MetadataId
        {
            get
            {
                var id = XrmService.GetFieldMetadata(FieldName, RecordType).MetadataId;
                return id != null ? id.ToString() : null;
            }
        }

        public string SchemaNameQualified
        {
            get
            {
                return string.Format("{0}.{1}", RecordType, SchemaName ?? DisplayName);
            }
        }

        public int Order
        {
            get
            {
                return 0;
            }
        }

        public bool IsMultiSelect
        {
            get
            {
                return XrmService.GetFieldMetadata(FieldName, RecordType) is MultiSelectPicklistAttributeMetadata;
            }
        }
    }
}
