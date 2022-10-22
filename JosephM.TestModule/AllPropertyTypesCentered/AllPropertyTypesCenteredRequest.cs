using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Test;
using JosephM.Record.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.TestModule.AllPropertyTypesCentered
{
    [Group(Sections.Main, Group.DisplayLayoutEnum.VerticalCentered)]
    [AllowSaveAndLoad]
    public class AllPropertyTypesCenteredRequest : ServiceRequestBase
    {
        public AllPropertyTypesCenteredRequest()
        {
            StringField = DateTime.Now.ToFileTime().ToString();
            MultilineStringField = (DateTime.Now.ToFileTime().ToString() + "\n").ReplicateString(10);
            EnumPicklist = AllPropertyTypesCompactEnum.Option100;
            EnumMultiselect = new[] { AllPropertyTypesCompactEnum.Option1, AllPropertyTypesCompactEnum.Option100 };
            Integer = 100;
            Boolean = true;
            Date = DateTime.Now;
            LookupField = FakeRecordService.Get().ToLookupWithAltDisplayNameName(FakeRecordService.Get().GetFirst(FakeConstants.RecordType));
            MultiLookupField = FakeRecordService.Get().ToLookupWithAltDisplayNameName(FakeRecordService.Get().GetFirst(FakeConstants.RecordType));
            Decimal = 200;
            Password = new Password("Fake", false, true);
            Folder = new Folder(CoreTest.TestingFolder);
            Double = 300;
            RecordType = new RecordType(FakeConstants.RecordType, FakeConstants.RecordType);
            Field = new RecordField(FakeConstants.LookupField, FakeConstants.LookupField);
            BigInt = 500;
            Hyperlink = new Url("http://www.google.com", "Google");
            SettingsLookup = TestSettingsTypeCentered.Create().Items.First();
            CsvFile = new FileReference(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestCsv.csv"));
        }

        [Group(Sections.Main)]
        public string StringField { get; set; }

        [Group(Sections.Main)]
        [Multiline]
        public string MultilineStringField { get; set; }

        [Group(Sections.Main)]
        public AllPropertyTypesCompactEnum EnumPicklist { get; set; }

        [Group(Sections.Main)]
        public IEnumerable<AllPropertyTypesCompactEnum> EnumMultiselect { get; set; }

        [Group(Sections.Main)]
        [SettingsLookup(typeof(TestSettingsTypeCentered), nameof(TestSettingsTypeCentered.Items))]
        public TestSettingsTypeCenteredEnumerated SettingsLookup { get; set; }

        [Group(Sections.Main)]
        public int Integer { get; set; }

        [Group(Sections.Main)]
        public bool Boolean { get; set; }

        [Group(Sections.Main)]
        public DateTime Date { get; set; }

        [Group(Sections.Main)]
        [ReferencedType(FakeConstants.RecordType)]
        public Lookup LookupField { get; set; }

        [Group(Sections.Main)]
        [ReferencedType(FakeConstants.RecordType)]
        [ReferencedType(FakeConstants.RecordType2)]
        public Lookup MultiLookupField { get; set; }

        [Group(Sections.Main)]
        public decimal Decimal { get; set; }

        [Group(Sections.Main)]
        public Password Password { get; set; }

        [Group(Sections.Main)]
        public Folder Folder { get; set; }

        [Group(Sections.Main)]
        public double Double { get; set; }

        [Group(Sections.Main)]
        [RecordTypeFor(nameof(Field))]
        public RecordType RecordType { get; set; }

        [Group(Sections.Main)]
        public RecordField Field { get; set; }

        [Group(Sections.Main)]
        public long BigInt { get; set; }

        [Group(Sections.Main)]
        [FileMask(FileMasks.CsvFile)]
        public FileReference CsvFile { get; set; }

        [Group(Sections.Main)]
        public Url Hyperlink { get; set; }

        [Group(Sections.Main)]
        public IEnumerable<AllPropertyTypesCenteredRequest> EnumerableField { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
        }

        public enum AllPropertyTypesCompactEnum
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
}