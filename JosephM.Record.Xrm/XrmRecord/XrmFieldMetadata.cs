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
            get
            {
                if (XrmService.GetEntityMetadata(RecordType).IsIntersect ?? false)
                {
                    var relationshipMetadata = XrmService.GetRelationshipMetadataForEntityName(RecordType);
                    var areSameEntity = relationshipMetadata.Entity1IntersectAttribute == relationshipMetadata.Entity2IntersectAttribute;
                    if (areSameEntity)
                    {
                        return FieldName;
                    }
                    if (relationshipMetadata.Entity1IntersectAttribute == FieldName)
                    {
                        return XrmService.GetEntityLabel(relationshipMetadata.Entity1LogicalName);
                    }
                    else if(relationshipMetadata.Entity2IntersectAttribute == FieldName)
                    {
                        return XrmService.GetEntityLabel(relationshipMetadata.Entity2LogicalName);
                    }
                }
                return XrmService.GetFieldLabel(FieldName, RecordType);
            }
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
                return FieldType == RecordFieldType.Money || FieldType == RecordFieldType.Decimal || FieldType == RecordFieldType.Double
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
                    case RecordFieldType.Decimal: return XrmService.GetMinDecimalValue(FieldName, RecordType);
                    case RecordFieldType.Double: return Convert.ToDecimal(XrmService.GetMinDoubleValue(FieldName, RecordType));
                    case RecordFieldType.Integer: return XrmService.GetMinIntValue(FieldName, RecordType);
                    case RecordFieldType.Money: return Convert.ToDecimal(XrmService.GetMinMoneyValue(FieldName, RecordType));
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
                    case RecordFieldType.Decimal: return XrmService.GetMaxDecimalValue(FieldName, RecordType);
                    case RecordFieldType.Double: return Convert.ToDecimal(XrmService.GetMaxDoubleValue(FieldName, RecordType));
                    case RecordFieldType.Integer: return XrmService.GetMaxIntValue(FieldName, RecordType);
                    case RecordFieldType.Money: return Convert.ToDecimal(XrmService.GetMaxMoneyValue(FieldName, RecordType));
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
                {
                    return false;
                }
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

        public string FormulaDefinition
        {
            get
            {
                var metadata = XrmService.GetFieldMetadata(FieldName, RecordType);
                if (metadata is BooleanAttributeMetadata bmt)
                    return bmt.FormulaDefinition;
                if (metadata is DateTimeAttributeMetadata dtmt)
                    return dtmt.FormulaDefinition;
                if (metadata is DecimalAttributeMetadata dmt)
                    return dmt.FormulaDefinition;
                if (metadata is IntegerAttributeMetadata imt)
                    return imt.FormulaDefinition;
                if (metadata is MoneyAttributeMetadata mmt)
                    return mmt.FormulaDefinition;
                if (metadata is PicklistAttributeMetadata pmt)
                    return pmt.FormulaDefinition;
                if (metadata is StringAttributeMetadata smt)
                    return smt.FormulaDefinition;
                return null;
            }
        }

        public string AutonumberFormat
        {
            get
            {
                var metadata = XrmService.GetFieldMetadata(FieldName, RecordType);
                if (metadata is StringAttributeMetadata smt)
                    return smt.AutoNumberFormat;
                return null;
            }
        }

        public bool HasFieldSecurity
        {
            get
            {
                var metadata = XrmService.GetFieldMetadata(FieldName, RecordType);
                return metadata.IsSecured.HasValue && metadata.IsSecured.Value;
            }
        }

        public string NavigationProperty
        {
            get
            {
                if (!XrmService.IsLookup(FieldName, RecordType))
                {
                    return null;
                }
                var relationships = XrmService.GetEntityManyToOneRelationships(RecordType);
                var relationshipMatches = relationships.Where(r => r.ReferencingAttribute == FieldName && r.ReferencingEntity == RecordType).ToArray();
                if (!relationshipMatches.Any())
                    return null;
                return string.Join(",", relationshipMatches.Select(r => r.ReferencingEntityNavigationPropertyName)
                    .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s));
            }
        }
    }
}
