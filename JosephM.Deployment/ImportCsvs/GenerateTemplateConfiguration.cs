using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportCsvs
{
    [Group(Sections.Main, true)]
    [BulkAddRecordTypeFunction]
    public class GenerateTemplateConfiguration
    {
        [RequiredProperty]
        [Group(Sections.Main)]
        [DisplayOrder(10)]
        [RecordTypeFor(nameof(FieldsToInclude) + "." + nameof(RecordField))]
        public RecordType RecordType { get; set; }

        [RequiredProperty]
        [Group(Sections.Main)]
        [DisplayOrder(20)]
        public bool AllFields { get; set; }

        [RequiredProperty]
        [Group(Sections.Main)]
        [DisplayOrder(30)]
        [PropertyInContextByPropertyValue(nameof(AllFields), false)]
        public IEnumerable<FieldSetting> FieldsToInclude { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
        }
    }
}
