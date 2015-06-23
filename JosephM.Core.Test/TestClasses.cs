using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;

namespace JosephM.Core.Test
{
    public static class TestConstants
    {
        public const string TestObjectClassDisplayName = "Object For Testing";
        public const string Enum1Description = "I Enum One";
        public const string TestFolder = @"C:\\Testing";

        public static string TestSettingsFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "JosephM Xrm", "Test Settings");
            }
        }
        public const string TestingString = "TestingString";
    }

    public interface ITestInterface
    {
    }

    [Attributes.DisplayName(TestConstants.TestObjectClassDisplayName)]
    public class TestObject : ITestInterface
    {
        public TestObject()
        {
        }

        public TestObject(string stringField, bool booleanField)
        {
            StringField = stringField;
            BooleanField = booleanField;
        }

        public TestObject(bool booleanField)
        {
            BooleanField = booleanField;
        }

        public TestObject(string stringField)
        {
            StringField = stringField;
        }

        public string StringField { get; set; }
        public DateTime? DateField { get; set; }
        public bool BooleanField { get; set; }
        public bool? BooleanNullableField { get; set; }

        [PropertyInContextByPropertyValue("BooleanNullableField", true)]
        public string ValidForBooleanNullableFieldTrue { get; set; }

        public TestEnum EnumField { get; set; }
        public TestEnum? EnumFieldNullable { get; set; }

        [PropertyInContextByPropertyValues("EnumFieldNullable", new object[] {TestEnum.Enum3, TestEnum.Enum4})]
        public string ValidForEnumFieldNullable3Or4 { get; set; }

        [PropertyInContextByPropertyNotNull("EnumFieldNullable")]
        public string ValidForEnumNullableNotNull { get; set; }

        [RequiredProperty]
        public string RequiredProperty { get; set; }

        public int IntField { get; set; }
        public IEnumerable<string> EnumerableStringField { get; set; }
        [FileMask(FileMasks.ExcelFile)]
        public FileReference ExcelFileField { get; set; }
        public string NoInContextAttributes { get; set; }

        private string _getSetField;

        public string SetOnly
        {
            set { _getSetField = value; }
        }

        public string GetOnly
        {
            get { return _getSetField; }
        }

        public string GetSet { get; set; }
    }

    public class TestObjectSubClass : TestObject
    {
    }

    public class NoEmptyConstructorObject
    {
        public NoEmptyConstructorObject(string stringField)
        {
            StringField = stringField;
        }

        public string StringField { get; set; }
    }

    public class TestObject2
    {
        public string StringField { get; set; }
        public DateTime? DateField { get; set; }
        public bool BooleanField { get; set; }
        public int IntField { get; set; }
    }

    public class TestObject3
    {
        public string StringField { get; set; }
        public DateTime? DateField { get; set; }
        public bool BooleanField { get; set; }
        public int IntField { get; set; }
    }

    public class TestObject4
    {
        public string StringField { get; set; }
        public DateTime? DateField { get; set; }
        public bool BooleanField { get; set; }
        public int IntField { get; set; }
    }

    public class TestObject5
    {
        public string StringField { get; set; }
        public DateTime? DateField { get; set; }
        public bool BooleanField { get; set; }
        public int IntField { get; set; }
    }

    public class TestObject6
    {
        public string StringField { get; set; }
        public DateTime? DateField { get; set; }
        public bool BooleanField { get; set; }
        public int IntField { get; set; }
    }

    public enum TestEnum
    {
        [Description(TestConstants.Enum1Description)] Enum1,
        Enum2,
        Enum3,
        Enum4
    }
}