
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System.Collections.Generic;

namespace JosephM.Deployment.ExportXml
{
    [BulkAddRecordTypeFunction]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Fields, true, 15)]
    [Group(Sections.Fetch, false, 20)]
    public class ExportRecordType
    {
        public ExportRecordType()
        {
            IncludeAllFields = true;
        }

        [GridWidth(140)]
        [DisplayOrder(0)]
        [Group(Sections.Main)]
        [RequiredProperty]
        public ExportType Type { get; set; }

        [DisplayOrder(10)]
        [PropertyInContextByPropertyNotNull(nameof(Type))]
        [Group(Sections.Main)]
        [RequiredProperty]
        [ReadOnlyWhenSet]
        [RecordTypeFor(nameof(IncludeOnlyTheseFields) + "." + nameof(RecordField))]
        [RecordTypeFor(nameof(SpecificRecordsToExport) + "." + nameof(LookupSetting.Record))]
        [RecordTypeFor(nameof(ExplicitValuesToSet) + "." + nameof(ExplicitFieldValues.FieldToSet))]
        public RecordType RecordType { get; set; }

        [GridWidth(100)]
        [DisplayOrder(30)]
        [Group(Sections.Fields)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public bool IncludeAllFields { get; set; }

        [GridWidth(300)]
        [DisplayOrder(40)]
        [Group(Sections.Fields)]
        [PropertyInContextByPropertyValue(nameof(IncludeAllFields), false)]
        [RequiredProperty]
        public IEnumerable<FieldSetting> IncludeOnlyTheseFields { get; set; }

        [GridWidth(300)]
        [DisplayOrder(45)]
        [FormEntry]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public IEnumerable<ExplicitFieldValues> ExplicitValuesToSet { get; set; }

        [GridWidth(300)]
        [DisplayOrder(50)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(Type), ExportType.SpecificRecords)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public IEnumerable<LookupSetting> SpecificRecordsToExport { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.Fetch)]
        [DisplayName("Fetch XML")]
        [Multiline]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(Type), ExportType.FetchXml)]
        public string FetchXml { get; set; }

        [GridWidth(100)]
        [DisplayOrder(110)]
        [PropertyInContextByPropertyNotNull(nameof(Type))]
        [Group(Sections.Main)]
        public bool IncludeInactive { get; set; }

        public override string ToString()
        {
            return RecordType == null ? "Null" : RecordType.Value;
        }

        private static class Sections
        {
            public const string Main = "Export Type Details";
            public const string Fields = "Fields To Include";
            public const string Fetch = "Fetch XML - Note Attributes in The Entered XML Will Be Ignored. All fields Will Be Included Apart From Those Selected For Exclusion";
        }

        [Group(Sections.FieldUpdate, Group.DisplayLayoutEnum.HorizontalWrap, 20)]
        [DoNotAllowGridEdit]
        public class ExplicitFieldValues
        {
            public override string ToString()
            {
                return
                    FieldToSet == null
                    ? base.ToString()
                    : FieldToSet.Value + " = " + (ClearValue ? "(null)" : ValueToSet?.ToString());
            }

            [Group(Sections.FieldUpdate)]
            [DisplayOrder(10)]
            [RequiredProperty]
            [RecordFieldFor(nameof(ValueToSet))]
            public RecordField FieldToSet { get; set; }

            [GridWidth(75)]
            [Group(Sections.FieldUpdate)]
            [DisplayOrder(20)]
            [RequiredProperty]
            [PropertyInContextByPropertyNotNull(nameof(FieldToSet))]
            public bool ClearValue { get; set; }

            [GridWidth(300)]
            [Group(Sections.FieldUpdate)]
            [DisplayOrder(30)]
            [RequiredProperty]
            [PropertyInContextByPropertyNotNull(nameof(FieldToSet))]
            [PropertyInContextByPropertyValue(nameof(ClearValue), false)]
            public object ValueToSet { get; set; }

            private static class Sections
            {
                public const string FieldUpdate = "Field Value To Set";
            }
        }
    }
}
