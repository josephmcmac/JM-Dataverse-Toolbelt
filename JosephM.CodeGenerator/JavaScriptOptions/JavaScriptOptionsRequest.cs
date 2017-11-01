using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System.Collections.Generic;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    [Group(Sections.OptionSetSelection, true, 10)]
    [Group(Sections.Other, true, 20)]
    public class JavaScriptOptionsRequest : ServiceRequestBase
    {
        [DisplayOrder(400)]
        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [RecordTypeFor(nameof(SpecificOptionSetField))]
        public RecordType RecordType { get; set; }

        [DisplayOrder(410)]
        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public bool AllOptionSetFields { get; set; }

        [DisplayOrder(420)]
        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(AllOptionSetFields), false)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [LookupCondition(nameof(IFieldMetadata.FieldType), ConditionType.In, new[] { RecordFieldType.Picklist, RecordFieldType.State, RecordFieldType.Status })]
        public RecordField SpecificOptionSetField { get; set; }

        [Group(Sections.Other)]
        [DisplayOrder(5000)]
        [RequiredProperty]
        public string NamespaceOfTheJavaScriptObject { get; set; }

        private static class Sections
        {
            public const string OptionSetSelection = "Select Details Of The Option Set Field(s) To Generate Constants For";
            public const string Other = "Other";
        }
    }
}