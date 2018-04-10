using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System;
using System.Collections.Generic;

namespace JosephM.Application.ViewModel.Test
{
    public class TestAllFieldTypes
    {
        public long BigIntField { get; set; }
        public bool BooleanField { get; set; }
        public DateTime DateField { get; set; }
        public decimal DecimalField { get; set; }
        public double DoubleField { get; set; }
        public IEnumerable<TestAllFieldTypes> EnumerableField { get; set; }
        public FileReference FileField { get; set; }
        public Folder FolderField { get; set; }
        public int Integerield { get; set; }
        public Money MoneyField { get; set; }
        public IEnumerable<TestEnum> PicklistMultiSelectField { get; set; }
        [SettingsLookup(typeof(SettingsTestAllFieldTypes), nameof(SettingsTestAllFieldTypes.SavedInstances))]
        public TestAllFieldTypes ObjectField { get; set; }
        public Password PasswordField { get; set; }
        public TestEnum PicklistField { get; set; }
        [ReferencedType(FakeConstants.RecordType)]
        [RecordTypeFor(nameof(RecordFieldField))]
        [RecordTypeFor(nameof(RecordFieldMultiSelectField))]
        [RecordTypeFor(nameof(LookupField))]
        public RecordType RecordTypeField { get; set; }
        public RecordField RecordFieldField { get; set; }
        public IEnumerable<RecordField> RecordFieldMultiSelectField { get; set; }
        public Lookup LookupField { get; set; }
        public string StringField { get; set; }
        public Url UrlField { get; set; }

        public override string ToString()
        {
            return StringField;
        }
    }

    public class SettingsTestAllFieldTypes
    {
        public IEnumerable<TestAllFieldTypes> SavedInstances { get; set; }
    }

    public enum TestEnum
    {
        Option1,
        Option2,
        Option3
    }
}
