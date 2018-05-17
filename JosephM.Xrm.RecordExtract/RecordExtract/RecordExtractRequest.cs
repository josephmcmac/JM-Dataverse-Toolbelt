using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.RecordExtract.DocumentWriter;
using System.Collections.Generic;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    [AllowSaveAndLoad]
    [Group(Sections.Document, true, 10)]
    [Group(Sections.RecordToReport, true, 20)]
    [Group(Sections.DetailLevelOptions, true, 30)]
    [Group(Sections.CommonFieldOptions, true, 40)]
    [Group(Sections.DisplayOptions, true, 45)]
    [Group(Sections.ConfigureReportExclusions, true, 50)]
    [DisplayName("Record Report")]
    public class RecordExtractRequest : ServiceRequestBase
    {
        public RecordExtractRequest()
        {
            StripHtmlTags = true;
            RecordTypesOnlyDisplayName = new RecordTypeSetting[0];
            FieldsToExclude = new RecordFieldSetting[0];
            RecordTypesToExclude = new RecordTypeSetting[0];
        }

        [DisplayOrder(10)]
        [Group(Sections.Document)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [DisplayOrder(20)]
        [Group(Sections.Document)]
        [RequiredProperty]
        public DocumentType DocumentFormat { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.RecordToReport)]
        [RequiredProperty]
        [RecordTypeFor(nameof(RecordLookup))]
        public RecordType RecordType { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.RecordToReport)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull("RecordType")]
        public Lookup RecordLookup { get; set; }

        [DisplayOrder(200)]
        [Group(Sections.DetailLevelOptions)]
        [RequiredProperty]
        public DetailLevel DetailOfRelatedRecords { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.DetailLevelOptions)]
        [PropertyInContextByPropertyValue("DetailOfRelatedRecords", DetailLevel.AllFields)]
        public IEnumerable<RecordTypeSetting> RecordTypesOnlyDisplayName { get; set; }

        [DisplayOrder(300)]
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeCreatedByAndOn { get; set; }
        [DisplayOrder(310)]
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeModifiedByAndOn { get; set; }
        [DisplayOrder(320)]
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeCrmOwner { get; set; }
        [DisplayOrder(330)]
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeState { get; set; }
        [DisplayOrder(340)]
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeStatus { get; set; }

        [DisplayOrder(380)]
        [Group(Sections.DisplayOptions)]
        public bool StripHtmlTags { get; set; }

        [DisplayOrder(385)]
        [Group(Sections.DisplayOptions)]
        [PropertyInContextByPropertyValue(nameof(StripHtmlTags), true)]
        public IEnumerable<RecordFieldSetting> CustomHtmlFields { get; set; }

        [DisplayOrder(400)]
        [Group(Sections.ConfigureReportExclusions)]
        public IEnumerable<RecordTypeSetting> RecordTypesToExclude { get; set; }

        [DisplayOrder(410)]
        [Group(Sections.ConfigureReportExclusions)]
        public IEnumerable<RecordFieldSetting> FieldsToExclude { get; set; }

        private static class Sections
        {
            public const string Document = "Document";
            public const string RecordToReport = "Record To Report";
            public const string CommonFieldOptions = "Common Field Options";
            public const string DetailLevelOptions = "Detail Level Options";
            public const string DisplayOptions = "Display Options";
            public const string ConfigureReportExclusions = "Configure Report Exclusions";
        }
    }
}