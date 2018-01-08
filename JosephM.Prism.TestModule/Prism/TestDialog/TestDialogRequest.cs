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
    [Group(Sections.DisplayFirst, true, 5)]
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

        public IEnumerable<TestEnum> MultiSelect { get; set; }

        [MyDescription("If set this will log a heap of errors in the service response")]
        [Group(Sections.Main)]
        public bool ThrowResponseErrors { get; set; }

        [Group(Sections.SelectAll)]
        public bool Bool1 { get; set; }

        [Group(Sections.SelectAll)]
        public bool Bool2 { get; set; }

        [Group(Sections.SelectAll)]
        public bool Bool3 { get; set; }

        //[Group(Sections.Setting)]
        //[SettingsLookup(typeof(ITestSettings), nameof(ITestSettings.Settings))]
        //public TestSetting TestSetting { get; set; }

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

        [Group(Sections.DisplayFirst)]
        [DoNotAllowAdd]
        public IEnumerable<TestDialogRequestItem> Items { get; set; }

        private static class Sections
        {
            public const string DisplayFirst = "DisplayFirst";
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

            public IEnumerable<TestEnum> MultiSelect { get; set; }

            [ReferencedType(FakeConstants.RecordType)]
            public IEnumerable<FieldSetting> Fields { get; set; }

            [ReferencedType(FakeConstants.RecordType)]
            public IEnumerable<LookupSetting> Lookups { get; set; }

            [ReadOnlyWhenSet]
            public string ReadOnlyProp { get; set; }
        }

        public enum TestEnum
        {
            Option1,
            Option2,
            Option3
        }
    }
} ;