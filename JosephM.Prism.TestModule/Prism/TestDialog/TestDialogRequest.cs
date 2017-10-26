using System.Collections;
using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Prism.TestModule.Prism.TestSettings;
using JosephM.Application.ViewModel.Fakes;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.SelectAll, true, order: 20, selectAll: true)]
    [Group(Sections.TypesAndLookups, true, order: 30)]
    [Group(Sections.Setting, true, order: 40)]
    [DisplayName("Test Dialog")]
    public class TestDialogRequest : ServiceRequestBase
    {
        public TestDialogRequest()
        {
            Items = new[] {new TestDialogRequestItem()};
        }

        [Group(Sections.Main)]
        public bool ThrowResponseErrors { get; set; }

        [Group(Sections.SelectAll)]
        public bool Bool1 { get; set; }

        [Group(Sections.SelectAll)]
        public bool Bool2 { get; set; }

        [Group(Sections.SelectAll)]
        public bool Bool3 { get; set; }

        [Group(Sections.Setting)]
        [SettingsLookup(typeof(ITestSettings), nameof(ITestSettings.Settings))]
        public TestSetting TestSetting { get; set; }

        [Group(Sections.TypesAndLookups)]
        [RecordTypeFor(nameof(Fields) + "." + nameof(FieldSetting.RecordField))]
        [RecordTypeFor(nameof(SpecificRecordsToExport) + "." + nameof(LookupSetting.Record))]
        public RecordType RecordType { get; set; }

        [Group(Sections.TypesAndLookups)]
        [ReferencedType(FakeConstants.RecordType)]
        [UsePicklist]
        [OrderPriority(FakeConstants.MainRecordName, "TestingString")]
        public Lookup LookupField { get; set; }

        [Group(Sections.TypesAndLookups)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public IEnumerable<FieldSetting> Fields { get; set; }

        [Group(Sections.TypesAndLookups)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public IEnumerable<LookupSetting> SpecificRecordsToExport { get; set; }

        [Group(Sections.TypesAndLookups)]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }


        [DoNotAllowAdd]
        public IEnumerable<TestDialogRequestItem> Items { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Setting = "Setting";
            public const string TypesAndLookups = "TypesAndLookups";
            public const string SelectAll = "SelectAll";
        }

        public class TestDialogRequestItem
        {
            public TestDialogRequestItem()
            {
                ReadOnlyProp = "I Read Only";
            }

            [ReadOnlyWhenSet]
            public string ReadOnlyProp { get; set; }
        }
    }
} ;