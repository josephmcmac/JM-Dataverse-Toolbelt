using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.Attributes;
using JosephM.Record.Metadata;
using JosephM.Xrm.MetadataImportExport;

namespace JosephM.Xrm.CustomisationExporter
{
    public class FieldExport
    {
        public FieldExport(string recordTypeLabel, string recordTypeSchemaName, string fieldLabel, string fieldSchemaName, RecordFieldType fieldType, bool isCustomField, bool isMandatory, string description, bool isPrimaryField, bool audit, bool searchable, bool displayInRelated, string referencedType, int maxLength, string textFormat, bool includeTime, string minValue, string maxValue, string decimalPrecision, string picklistOptions)
        {
            DecimalPrecision = decimalPrecision;
            RecordTypeSchemaName = recordTypeSchemaName;
            FieldSchemaName = fieldSchemaName;
            FieldLabel = fieldLabel;
            FieldType = fieldType;
            Description = description;
            IsPrimaryField = isPrimaryField;
            IsMandatory = isMandatory;
            Audit = audit;
            Searchable = searchable;
            ReferencedTypeSchemaName = referencedType;
            DisplayInRelated = displayInRelated;
            MaxLength = maxLength;
            TextFormat = textFormat;
            IncludeTime = includeTime;
            MinValue = minValue;
            MaxValue = maxValue;
            PicklistOptions = picklistOptions;

            RecordTypeLabel = recordTypeLabel;
            IsCustomField = isCustomField;
        }

        [DisplayName(Headings.Fields.RecordTypeSchemaName)]
        public string RecordTypeSchemaName { get; set; }
        [DisplayName(Headings.Fields.SchemaName)]
        public string FieldSchemaName { get; set; }
        [DisplayName(Headings.Fields.DisplayName)]
        public string FieldLabel { get; set; }
        [DisplayName(Headings.Fields.FieldType)]
        public RecordFieldType FieldType { get; set; }
        [DisplayName(Headings.Fields.Description)]
        public string Description { get; set; }
        [DisplayName(Headings.Fields.IsPrimaryField)]
        public bool IsPrimaryField { get; set; }
        [DisplayName(Headings.Fields.IsMandatory)]
        public bool IsMandatory { get; set; }
        [DisplayName(Headings.Fields.Audit)]
        public bool Audit { get; set; }
        [DisplayName(Headings.Fields.Searchable)]
        public bool Searchable { get; set; }

        [DisplayName(Headings.Fields.ViewOrder)]
        public string ViewOrder { get; set; }
        [DisplayName(Headings.Fields.ViewWidth)]
        public string ViewWidth { get; set; }

        [DisplayName(Headings.Fields.ReferencedRecordType)]
        public string ReferencedTypeSchemaName { get; set; }
        [DisplayName(Headings.Fields.DisplayInRelated)]
        public bool DisplayInRelated { get; set; }
        [DisplayName(Headings.Fields.MaxLength)]
        public int MaxLength { get; set; }
        [DisplayName(Headings.Fields.TextFormat)]
        public string TextFormat { get; set; }
        [DisplayName(Headings.Fields.IncludeTime)]
        public bool IncludeTime { get; set; }
        [DisplayName(Headings.Fields.Minimum)]
        public string MinValue { get; set; }
        [DisplayName(Headings.Fields.Maximum)]
        public string MaxValue { get; set; }
        [DisplayName(Headings.Fields.DecimalPrecision)]
        public string DecimalPrecision { get; set; }
        [DisplayName(Headings.Fields.PicklistOptions)]
        public string PicklistOptions { get; set; }
        public string RecordTypeLabel { get; set; }
        public bool IsCustomField { get; set; }
    }
}
