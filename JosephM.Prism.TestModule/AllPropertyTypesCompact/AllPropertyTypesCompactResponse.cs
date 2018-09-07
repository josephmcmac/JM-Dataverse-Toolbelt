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

namespace JosephM.TestModule.AllPropertyTypesCompact
{
    [Group(Sections.Main, true, 10)]
    public class AllPropertyTypesCompactResponse : ServiceResponseBase<AllPropertyTypesCompactResponseItem>
    {
        public AllPropertyTypesCompactResponse()
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
            SettingsLookup = TestSettingsTypeCompact.Create().Items.First();
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
        [SettingsLookup(typeof(TestSettingsTypeCompact), nameof(TestSettingsTypeCompact.Items))]
        public TestSettingsTypeCompactEnumerated SettingsLookup { get; set; }

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
        public IEnumerable<AllPropertyTypesCompactResponse> EnumerableField { get; set; }

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
        }
    }
}