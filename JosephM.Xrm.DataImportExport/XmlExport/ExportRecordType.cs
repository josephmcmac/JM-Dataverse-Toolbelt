using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using JosephM.Xrm.DataImportExport.Import;

namespace JosephM.Xrm.DataImportExport.XmlImport
{
    [BulkAddRecordTypeFunction(true)]
    [Group(Sections.Main, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.Fields, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 15)]
    [Group(Sections.Fetch, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20)]
    public partial class ExportRecordType
    {
        public ExportRecordType()
        {
            IncludeAllFields = true;
        }

        [MyDescription("Type Of Query For Identifying Records To Be Included")]
        [GridWidth(140)]
        [DisplayOrder(10)]
        [Group(Sections.Main)]
        [RequiredProperty]
        public ExportType Type { get; set; }

        [MyDescription("Type Of Records To Include")]
        [DisplayOrder(0)]
        [PropertyInContextByPropertyNotNull(nameof(Type))]
        [Group(Sections.Main)]
        [RequiredProperty]
        [ReadOnlyWhenSet]
        [RecordTypeExclusions(Entities.activitypointer, Entities.workflow, Entities.asyncoperation, Entities.pluginassembly, Entities.plugintracelog, Entities.plugintype, Entities.plugintypestatistic, Entities.tracelog, Entities.convertrule, Entities.importjob, Entities.emailserverprofile, Entities.sharepointdocument, Entities.serviceendpoint, Entities.role, Entities.sdkmessage, Entities.sdkmessagefilter, Entities.sdkmessageprocessingstepimage, Entities.sdkmessageprocessingstepsecureconfig, Entities.routingrule)]
        [RecordTypeFor(nameof(IncludeOnlyTheseFields) + "." + nameof(RecordField))]
        [RecordTypeFor(nameof(SpecificRecordsToExport) + "." + nameof(LookupSetting.Record))]
        [RecordTypeFor(nameof(ExplicitValuesToSet) + "." + nameof(ExplicitFieldValues.FieldToSet))]
        public RecordType RecordType { get; set; }

        [MyDescription("If Set All Fields Will Be Included In The Export")]
        [GridWidth(100)]
        [DisplayOrder(30)]
        [Group(Sections.Fields)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public bool IncludeAllFields { get; set; }

        [MyDescription("If Not Including All Fields This Defines The Fields To Include")]
        [GridWidth(300)]
        [DisplayOrder(40)]
        [Group(Sections.Fields)]
        [PropertyInContextByPropertyValue(nameof(IncludeAllFields), false)]
        [RequiredProperty]
        public IEnumerable<FieldSetting> IncludeOnlyTheseFields { get; set; }

        [MyDescription("This Allows Setting A Specific Value For A Field In All Records Exported")]
        [GridWidth(300)]
        [DisplayOrder(45)]
        [FormEntry]
        [AllowNestedGridEdit]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public IEnumerable<ExplicitFieldValues> ExplicitValuesToSet { get; set; }

        [MyDescription("If Type = Specific Records This Defines The Records To Export")]
        [GridWidth(300)]
        [DisplayOrder(50)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(Type), ExportType.SpecificRecords)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public IEnumerable<LookupSetting> SpecificRecordsToExport { get; set; }

        [MyDescription("If Type = FetchXml This Defines The Fetch Xml Query To Use")]
        [DisplayOrder(100)]
        [Group(Sections.Fetch)]
        [DisplayName("Fetch XML")]
        [Multiline]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(Type), ExportType.FetchXml)]
        public string FetchXml { get; set; }

        [MyDescription("If Not Set Inactive Records Will Not Be Included")]
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
    }
}
