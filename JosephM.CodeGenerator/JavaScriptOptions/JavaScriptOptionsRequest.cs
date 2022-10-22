using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    [DisplayName("JavaScript Option Generation")]
    [Instruction("JavaScript will be output with properties for picklist options. This may then be copied into a JavaScriot file for referencing option values")]
    [Group(Sections.Type, Group.DisplayLayoutEnum.VerticalCentered, order:10)]
    [Group(Sections.Fields, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20)]
    public class JavaScriptOptionsRequest : ServiceRequestBase
    {
        [DisplayOrder(400)]
        [Group(Sections.Type)]
        [RequiredProperty]
        [RecordTypeFor(nameof(SpecificOptionSetField))]
        public RecordType RecordType { get; set; }

        [DisplayOrder(410)]
        [Group(Sections.Fields)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public bool AllOptionSetFields { get; set; }

        [DisplayOrder(420)]
        [Group(Sections.Fields)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(AllOptionSetFields), false)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [LookupCondition(nameof(IFieldMetadata.FieldType), ConditionType.In, new[] { RecordFieldType.Picklist, RecordFieldType.State, RecordFieldType.Status })]
        public RecordField SpecificOptionSetField { get; set; }

        private static class Sections
        {
            public const string Type = "Select detail of the picklist values to generate constants for";
            public const string Fields = "Fields";
        }
    }
}