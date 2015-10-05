using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.Mappers;
using JosephM.Xrm;
using Microsoft.Xrm.Sdk.Metadata;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmFieldMetadata : XrmConfigurationBase, IFieldMetadata
    {
        private string FieldName { get; set; }
        private string RecordType { get; set; }

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
            get { return new FieldTypeMapper().Map(XrmService.GetFieldType(FieldName, RecordType)); }
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
            get { return XrmService.GetMaxLength(FieldName, RecordType); }
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

        public bool IncludeTime
        {
            get { return XrmService.IsDateIncludeTime(FieldName, RecordType); }
        }

        public TextFormat TextFormat
        {
            get
            {
                var format = XrmService.GetTextFormat(FieldName, RecordType);
                return
                    format == null
                        ? TextFormat.Text
                        : new StringFormatMapper().Map((StringFormat) format);
            }
        }

        public int DecimalPrecision
        {
            get { return XrmService.GetPrecision(FieldName, RecordType); }
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
                throw new NotImplementedException(string.Format("Not implemented for field {0} in {1} of type {2}", FieldName, RecordType, FieldType));
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
                throw new NotImplementedException(string.Format("Not implemented for field {0} in {1} of type {2}", FieldName, RecordType, FieldType));
            }
        }

        public IntegerType IntegerFormat
        {
            get { return new IntegerTypeMapper().Map(XrmService.GetIntegerFormat(FieldName, RecordType)); }
        }

        public bool IsNonNullable { get { return false; } }

        public bool IsSharedPicklist
        {
            get
            {
                return ((PicklistAttributeMetadata)XrmService.GetFieldMetadata(FieldName, RecordType)).OptionSet.IsGlobal ?? false;
            }
        }

        public string PicklistName
        {
            get
            {
                var name = ((PicklistAttributeMetadata)XrmService.GetFieldMetadata(FieldName, RecordType)).OptionSet.Name;
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
            get { return XrmService.IsWritable(FieldName, RecordType); }
        }

        public bool Createable
        {
            get { return XrmService.IsCreateable(FieldName, RecordType); }
        }

        public bool IsDisplayRelated
        {
            get
            {
                var relationships = XrmService.GetEntityManyToOneRelationships(RecordType);
                var relationshipMatches = relationships.Where(r => r.ReferencingAttribute == FieldName && r.ReferencingEntity == RecordType).ToArray();
                if (!relationshipMatches.Any())
                    return false;
                return relationshipMatches.Any(r => r.AssociatedMenuConfiguration != null && GetIsDisplayRelated(r.AssociatedMenuConfiguration));
            }
        }
    }
}
