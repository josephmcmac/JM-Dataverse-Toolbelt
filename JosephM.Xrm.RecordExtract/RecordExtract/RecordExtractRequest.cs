using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.RecordExtract.DocumentWriter;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    [Group(Sections.Document, true)]
    [Group(Sections.RecordToReport, true)]
    [Group(Sections.DetailLevelOptions, true)]
    [Group(Sections.CommonFieldOptions, true)]
    [Group(Sections.ConfigureReportExclusions, true)]
    [DisplayName("Record Report")]
    public class RecordExtractRequest : ServiceRequestBase
    {
        public RecordExtractRequest()
        {
            RecordTypesOnlyDisplayName = new RecordTypeSetting[0];
            FieldsToExclude = new RecordFieldSetting[0];
            RecordTypesToExclude = new RecordTypeSetting[0];
        }

        [Group(Sections.Document)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [Group(Sections.Document)]
        [RequiredProperty]
        public DocumentType DocumentFormat { get; set; }

        [Group(Sections.RecordToReport)]
        [RequiredProperty]
        [RecordTypeFor("RecordLookup")]
        public RecordType RecordType { get; set; }

        [Group(Sections.RecordToReport)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull("RecordType")]
        public Lookup RecordLookup { get; set; }

        [Group(Sections.DetailLevelOptions)]
        [RequiredProperty]
        public DetailLevel DetailOfRelatedRecords { get; set; }

        [Group(Sections.DetailLevelOptions)]
        [PropertyInContextByPropertyValue("DetailOfRelatedRecords", DetailLevel.AllFields)]
        public IEnumerable<RecordTypeSetting> RecordTypesOnlyDisplayName { get; set; }

        [Group(Sections.CommonFieldOptions)]
        public bool IncludeCreatedByAndOn { get; set; }
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeModifiedByAndOn { get; set; }
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeCrmOwner { get; set; }
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeState { get; set; }
        [Group(Sections.CommonFieldOptions)]
        public bool IncludeStatus { get; set; }

        [Group(Sections.ConfigureReportExclusions)]
        public IEnumerable<RecordTypeSetting> RecordTypesToExclude { get; set; }

        [Group(Sections.ConfigureReportExclusions)]
        public IEnumerable<RecordFieldSetting> FieldsToExclude { get; set; }

        private static class Sections
        {
            public const string Document = "Document";
            public const string RecordToReport = "Record To Report";
            public const string CommonFieldOptions = "Common Field Options";
            public const string DetailLevelOptions = "Detail Level Options";
            public const string ConfigureReportExclusions = "Configure Report Exclusions";
        }
    }
}