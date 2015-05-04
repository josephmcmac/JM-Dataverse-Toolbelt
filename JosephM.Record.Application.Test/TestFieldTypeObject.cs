using System;
using JosephM.Core.FieldType;

namespace JosephM.Record.Application.Test
{
    public class TestFieldTypeObject
    {
        public string StringProperty { get; set; }
        public TestFieldTypeEnum EnumProperty { get; set; }
        public int IntProperty { get; set; }
        public bool BooleanProperty { get; set; }
        public ExcelFile ExcelFileProperty { get; set; }
        public DateTime DatetimeProperty { get; set; }
        public decimal DecimalProperty { get; set; }
        public Password PasswordProperty { get; set; }
        public Folder FolderProperty { get; set; }
        public Double DoubleProperty { get; set; }

        public class TestNestedTypeObject
        {
            public string StringProperty { get; set; }
            public TestFieldTypeEnum EnumProperty { get; set; }
            public int IntProperty { get; set; }
            public bool BooleanProperty { get; set; }
            public DateTime DatetimeProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public Double DoubleProperty { get; set; }
        }

        public enum TestFieldTypeEnum
        {
            Option1,
            Option2
        }
    }
}
