using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Excel;
using System.Collections.Generic;

namespace JosephM.TestModule.TestDialog
{
    [GridOnlyEntry(nameof(Items))]
    [AllowSaveAndLoad]
    [Group(Sections.DisplayFirst, true, 5)]
    [Group(Sections.DisplaySecond, true, 5)]
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

        [DisplayOrder(15)]
        [Group(Sections.Main)]
        [FileMask(FileMasks.ExcelFile)]
        [ConnectionFor(nameof(ExcelRecordType), typeof(ExcelFileConnection))]
        [ConnectionFor(nameof(ExcelField), typeof(ExcelFileConnection))]
        public FileReference ExcelFile { get; set; }

        [PropertyInContextByPropertyNotNull(nameof(ExcelFile))]
        [RecordTypeFor(nameof(ExcelField))]
        [DisplayOrder(16)]
        [Group(Sections.Main)]
        public RecordType ExcelRecordType { get; set; }

        [PropertyInContextByPropertyNotNull(nameof(ExcelRecordType))]
        [DisplayOrder(17)]
        [Group(Sections.Main)]
        public RecordField ExcelField { get; set; }


        [MyDescription("If set this will log a heap of errors in the service response")]
        [Group(Sections.Main)]
        public bool ThrowFatalErrors { get; set; }

        [MyDescription("If set this will log a heap of errors in the service response")]
        [Group(Sections.Main)]
        public bool ThrowResponseErrors { get; set; }

        [MyDescription("Will Wait Halfway Through Completion")]
        [Group(Sections.Main)]
        public bool Wait10SecondsHalfwayThrough { get; set; }

        [Group(Sections.Main)]
        [FileMask(FileMasks.ZipFile)]
        public FileReference ZipFile { get; set; }

        [Group(Sections.Main)]
        [FileMask(FileMasks.XmlFile)]
        public FileReference XmlFile { get; set; }

        [Group(Sections.Main)]
        public Folder Folder { get; set; }

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
        public string AutocompleteField { get; set; }

        [Group(Sections.DisplaySecond)]
        [DoNotAllowAdd]
        public IEnumerable<TestDialogRequestItem> Items { get; set; }

        private static class Sections
        {
            public const string DisplayFirst = "DisplayFirst";
            public const string DisplaySecond = "DisplaySecond";
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

            public string AutocompleteField { get; set; }

            public IEnumerable<TestEnum> MultiSelect { get; set; }

            [ReferencedType(FakeConstants.RecordType)]
            public IEnumerable<FieldSetting> Fields { get; set; }

            [ReferencedType(FakeConstants.RecordType)]
            public IEnumerable<LookupSetting> Lookups { get; set; }

            [ReadOnlyWhenSet]
            public string ReadOnlyProp { get; set; }

            [FileMask(FileMasks.CsvFile)]
            public FileReference CsvFile { get; set; }
        }

        public enum TestEnum
        {
            Option1,
            Option2,
            Option3,
            Option4,
            Option5,
            Option6,
            Option7,
            Option8,
            Option9,
            Option10,
            Option11,
            Option12,
            Option13,
            Option14,
            Option15,
            Option16,
            Option17,
            Option18,
            Option19,
            Option20,
            Option21,
            Option22,
            Option23,
            Option24,
            Option25,
            Option26,
            Option27,
            Option28,
            Option29,
            Option30,
            Option31,
            Option32,
            Option33,
            Option34,
            Option35,
            Option36,
            Option37,
            Option38,
            Option39,
            Option40,
            Option41,
            Option42,
            Option43,
            Option44,
            Option45,
            Option46,
            Option47,
            Option48,
            Option49,
            Option50,
            Option51,
            Option52,
            Option53,
            Option54,
            Option55,
            Option56,
            Option57,
            Option58,
            Option59,
            Option60,
            Option61,
            Option62,
            Option63,
            Option64,
            Option65,
            Option66,
            Option67,
            Option68,
            Option69,
            Option70,
            Option71,
            Option72,
            Option73,
            Option74,
            Option75,
            Option76,
            Option77,
            Option78,
            Option79,
            Option80,
            Option81,
            Option82,
            Option83,
            Option84,
            Option85,
            Option86,
            Option87,
            Option88,
            Option89,
            Option90,
            Option91,
            Option92,
            Option93,
            Option94,
            Option95,
            Option96,
            Option97,
            Option98,
            Option99,
            Option100,
            Option101,
            Option102,
            Option103,
            Option104,
            Option105,
            Option106,
            Option107,
            Option108,
            Option109,
            Option110,
            Option111,
            Option112,
            Option113,
            Option114,
            Option115,
            Option116,
            Option117,
            Option118,
            Option119,
            Option120,
            Option121,
            Option122,
            Option123,
            Option124,
            Option125,
            Option126,
            Option127,
            Option128,
            Option129,
            Option130,
            Option131,
            Option132,
            Option133,
            Option134,
            Option135,
            Option136,
            Option137,
            Option138,
            Option139,
            Option140,
            Option141,
            Option142,
            Option143,
            Option144,
            Option145,
            Option146,
            Option147,
            Option148,
            Option149,
            Option150,
            Option151,
            Option152,
            Option153,
            Option154,
            Option155,
            Option156,
            Option157,
            Option158,
            Option159,
            Option160,
            Option161,
            Option162,
            Option163,
            Option164,
            Option165,
            Option166,
            Option167,
            Option168,
            Option169,
            Option170,
            Option171,
            Option172,
            Option173,
            Option174,
            Option175,
            Option176,
            Option177,
            Option178,
            Option179,
            Option180,
            Option181,
            Option182,
            Option183,
            Option184,
            Option185,
            Option186,
            Option187,
            Option188,
            Option189,
            Option190,
            Option191,
            Option192,
            Option193,
            Option194,
            Option195,
            Option196,
            Option197,
            Option198,
            Option199,
            Option200,
            Option201,
            Option202,
            Option203,
            Option204,
            Option205,
            Option206,
            Option207,
            Option208,
            Option209,
            Option210,
            Option211,
            Option212,
            Option213,
            Option214,
            Option215,
            Option216,
            Option217,
            Option218,
            Option219,
            Option220,
            Option221,
            Option222,
            Option223,
            Option224,
            Option225,
            Option226,
            Option227,
            Option228,
            Option229,
            Option230,
            Option231,
            Option232,
            Option233,
            Option234,
            Option235,
            Option236,
            Option237,
            Option238,
            Option239,
            Option240,
            Option241,
            Option242,
            Option243,
            Option244,
            Option245,
            Option246,
            Option247,
            Option248,
            Option249,
            Option250,
            Option251,
            Option252,
            Option253,
            Option254,
            Option255,
            Option256
        }
    }
} ;